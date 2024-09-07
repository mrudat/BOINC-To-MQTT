// <copyright file="IntegrationTests.cs" company="Martin Rudat">
// BOINC To MQTT - Exposes some BOINC controls via MQTT for integration with Home Assistant.
// Copyright (C) 2024  Martin Rudat
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>

namespace BOINC_To_MQTT.Tests;

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using BOINC_To_MQTT.Tests.Scaffolding;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Testcontainers;
using Testcontainers.BOINC;
using Testcontainers.HomeAssistant;
using Xunit.Abstractions;

public class IntegrationTests(ITestOutputHelper testOutputHelper)
{
    public static readonly TheoryData<string> Fixtures = MqttTestData.Implementing<IMqttContainer>();

    private readonly ILogger<IntegrationTests> logger = XUnitLogger.CreateLogger<IntegrationTests>(testOutputHelper);

    [Theory]
    [MemberData(nameof(Fixtures))]
    public async Task Test1Async(string containerTypeName)
    {
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        this.logger.LogInformation("Starting test setup...");

        // TODO start a test project server (https://github.com/BOINC/boinc-server-test), so we can register some test projects?
        await using var boincContainer = new BoincBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "B>"))
            .Build();

        this.logger.LogDebug("BOINC container initialized.");

        var boincStarted = boincContainer.StartAsync();

        await using var network = new NetworkBuilder()
            .Build();

        this.logger.LogInformation("Network created.");

        await using var mqttContainer = MqttTestData.GetNewContainer(
            containerTypeName,
            this.logger,
            testOutputHelper,
            network: network,
            networkAlias: "mqtt-server",
            prefix: "M>");

        this.logger.LogDebug("MQTT container initialized.");

        await using var homeAssistantContainer = new HomeAssistantBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "H>"))
            .WithNetwork(network)
            .Build();

        this.logger.LogInformation("Home Assistant container initialized.");

        var mqttStarted = mqttContainer.StartAsync();

        var homeAssistantStarted = homeAssistantContainer.StartAsync();

        var homeAssistantConfigured = this.ConfigureHomeAssistant(mqttContainer, homeAssistantContainer, mqttStarted, homeAssistantStarted, cancellationToken);

        var builder = Program.CreateApplicationBuilder([]);

        MockFileSystem mockFileSystem = new();

        mockFileSystem
            .AddDirectory(builder.Environment.ContentRootPath);

        builder.Services
            .AddSingleton<IFileSystem>(mockFileSystem);

        builder.Logging
            .AddProvider(new XUnitLoggerProvider(testOutputHelper))
            .SetMinimumLevel(LogLevel.Debug);

        await Task.WhenAll(
            boincStarted,
            mqttStarted);

        this.logger.LogInformation("BOINC and MQTT containers started.");

        var configuration = new Dictionary<string, string?>()
        {
            { "BOINC2MQTT:MQTT:URI", mqttContainer.GetMqttUri().ToString() },
            { "BOINC2MQTT:Remote:0:BoincUri", new UriBuilder("tcp", boincContainer.Hostname, boincContainer.GuiRpcPort).ToString() },
            { "BOINC2MQTT:Remote:0:GuiRpcKey", await boincContainer.GetGuiRpcKeyAsync() },
        };

        builder.Configuration
            .AddInMemoryCollection(configuration);

        using var host = builder.Build();

        var cancellationTokenSource = new CancellationTokenSource();

        await Task.WhenAll(
            homeAssistantStarted,
            homeAssistantConfigured);

        this.logger.LogInformation("Home Assistant started and configured.");

        var theTestTask = Task.Run(async () => await this.TheTest(cancellationTokenSource, mqttContainer, homeAssistantContainer));

        await Task.WhenAll(
            host.RunAsync(cancellationTokenSource.Token),
            theTestTask);
    }

    private async Task TheTest(CancellationTokenSource cancellationTokenSource, IMqttContainer mqttContainer, HomeAssistantContainer homeAssistantContainer)
    {
        var cancellationToken = cancellationTokenSource.Token;

        var waitForMqttTask = this.WaitForMqtt(mqttContainer, cancellationToken);

        var waitForSerivceDiscoveryTask = this.WaitForSerivceDiscovery(homeAssistantContainer, cancellationToken);

        await Task.WhenAll(waitForMqttTask, waitForSerivceDiscoveryTask);

        this.logger.LogInformation("Test setup done.");

        // TODO interact with home assistant, and check BOINC

        bool forever = false;

        if (!forever)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            await Task.Yield();
        }
        else
        {
            bool done = false;

            while (!done)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await Task.Yield();
            }
        }

        this.logger.LogInformation("Test done, shutting things down.");
        await cancellationTokenSource.CancelAsync();
    }

    private async Task WaitForSerivceDiscovery(HomeAssistantContainer homeAssistantContainer, CancellationToken cancellationToken)
    {
        var action = async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await homeAssistantContainer.TemplateAsync(
                    "{{ states.number | count }}",
                    cancellationToken);

                if (!result.Equals("0"))
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        };

        await action.Should().CompleteWithinAsync(TimeSpan.FromSeconds(60));

        this.logger.LogInformation("Service Discovery complete.");
    }

    private async Task WaitForMqtt(IMqttContainer mqttContainer, CancellationToken cancellationToken)
    {
        var mqttFactory = new MqttFactory();

        var homeAssistantIsOnline = new TaskCompletionSource();

        using var mqttClient = mqttFactory.CreateMqttClient();

        async Task WaitForMessage(MqttApplicationMessageReceivedEventArgs args)
        {
            if (args.ApplicationMessage.Topic != "homeassistant/status")
            {
                return;
            }

            if (!args.ApplicationMessage.ConvertPayloadToString().Equals("online"))
            {
                return;
            }

            mqttClient.ApplicationMessageReceivedAsync -= WaitForMessage;
            homeAssistantIsOnline.TrySetResult();
            await Task.Yield();
        }

        mqttClient.ApplicationMessageReceivedAsync += WaitForMessage;

        if (mqttContainer is IRequiresAuthentication requiresAuthentication)
        {
            await requiresAuthentication.AddUser("waitUser", "waitUser", cancellationToken);
        }

        var mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
            .WithConnectionUri(mqttContainer.GetMqttUri("waitUser"))
            .Build();
        await mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);

        await mqttClient.SubscribeAsync("homeassistant/status", cancellationToken: cancellationToken);

        await homeAssistantIsOnline.Invoking(async (haio) => await haio.Task).Should().CompleteWithinAsync(TimeSpan.FromSeconds(60));

        this.logger.LogInformation("MQTT status is online.");
    }

    private async Task ConfigureHomeAssistant(IMqttContainer mqttContainer, HomeAssistantContainer homeAssistantContainer, Task mqttStarted, Task homeAssistantStarted, CancellationToken cancellationToken)
    {
        await homeAssistantStarted;

        this.logger.LogDebug("Home Assistant started and configured.");

        await homeAssistantContainer.PerformOnBoardingAsync(cancellationToken);

        await mqttStarted;

        if (mqttContainer is IRequiresAuthentication requiresAuthentication)
        {
            await requiresAuthentication.AddUser("homeassistant", "homeassistant", cancellationToken);
            this.logger.LogDebug("MQTT user added.");
        }

        await homeAssistantContainer.ConfigureMqttAsync(mqttContainer, userName: "homeassistant", cancellationToken: cancellationToken);

        var client = homeAssistantContainer.WebUiHttpClient;

        var result = await client.GetAsync("/api/", cancellationToken);

        result.IsSuccessStatusCode.Should().Be(true);
        this.logger.LogDebug("Home Assistant API call successful.");
    }
}
