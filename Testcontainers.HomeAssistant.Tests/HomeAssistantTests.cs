// <copyright file="HomeAssistantTests.cs" company="Martin Rudat">
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

namespace Testcontainers.HomeAssistant.Tests;

using System.Diagnostics;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Testcontainers.Mosquitto;
using Xunit.Abstractions;

public class HomeAssistantTests(ITestOutputHelper testOutputHelper)
{
    private readonly ILogger<HomeAssistantTests> logger = XUnitLogger.CreateLogger<HomeAssistantTests>(testOutputHelper);

    [Fact]
    public async Task TestCanConfigureMqtt()
    {
        await using var network = new NetworkBuilder()
            .WithLogger(this.logger)
            .Build();

        await using var homeAssistantContainer = new HomeAssistantBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "H>"))
            .WithNetwork(network)
            .WithNetworkAliases("home-assistant")
            .Build();

        await using var mqttContainer = new MosquittoBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "M>"))
            .WithNetwork(network)
            .WithNetworkAliases("mosquitto")
            .Build();

        await using var mqttIsOline = new HomeAssistantWaitForMqttOnline();

        await homeAssistantContainer.StartAsync();

        await homeAssistantContainer.PerformOnBoardingAsync();

        await mqttContainer.StartAsync();

        await mqttIsOline.StartAsync(((IMqttContainer)mqttContainer).GetMqttUri());

        var stopWatch = new Stopwatch();

        await homeAssistantContainer.ConfigureMqttAsync(mqttContainer);

        stopWatch.Start();

        await mqttIsOline.Invoking(async (mio) => await mio.Wait()).Should().CompleteWithinAsync(TimeSpan.FromSeconds(30));

        stopWatch.Stop();

        var elapsed = stopWatch.Elapsed;

        testOutputHelper.WriteLine("Took {0:g} for HomeAssistant MQTT Integration to come online", elapsed);
    }

    [Fact]
    public async Task TestCanConfigureMqttTwice()
    {
        await using var network = new NetworkBuilder()
            .WithLogger(this.logger)
            .Build();

        await using var container = new HomeAssistantBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "H>"))
            .WithNetwork(network)
            .WithNetworkAliases("home-assistant")
            .Build();

        await using var mqttContainer = new MosquittoBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "M>"))
            .WithNetwork(network)
            .WithNetworkAliases("mosquitto")
            .Build();

        await using var mqttIsOline = new HomeAssistantWaitForMqttOnline();

        await container.StartAsync();

        await container.PerformOnBoardingAsync();

        await mqttContainer.StartAsync();

        await mqttIsOline.StartAsync(((IMqttContainer)mqttContainer).GetMqttUri());

        var stopWatch = new Stopwatch();

        await container.ConfigureMqttAsync(mqttContainer);

        await container.ConfigureMqttAsync(mqttContainer);

        stopWatch.Start();

        await mqttIsOline.Invoking(async (mio) => await mio.Wait()).Should().CompleteWithinAsync(TimeSpan.FromSeconds(30));

        stopWatch.Stop();

        var elapsed = stopWatch.Elapsed;

        testOutputHelper.WriteLine("Took {0:g} for HomeAssistant MQTT Integration to come online", elapsed);
    }

    [Fact]
    public async Task TestCanConfigureMqttWithoutExplicitOnBoarding()
    {
        await using var network = new NetworkBuilder()
            .WithLogger(this.logger)
            .Build();

        await using var container = new HomeAssistantBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "H>"))
            .WithNetwork(network)
            .WithNetworkAliases("home-assistant")
            .Build();

        await using var mqttContainer = new MosquittoBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "M>"))
            .WithNetwork(network)
            .WithNetworkAliases("mosquitto")
            .Build();

        await using var mqttIsOline = new HomeAssistantWaitForMqttOnline();

        var stopWatch = new Stopwatch();

        await container.StartAsync();

        await mqttContainer.StartAsync();

        await mqttIsOline.StartAsync(((IMqttContainer)mqttContainer).GetMqttUri());

        await container.ConfigureMqttAsync(mqttContainer);

        stopWatch.Start();

        await mqttIsOline.Invoking(async (mio) => await mio.Wait()).Should().CompleteWithinAsync(TimeSpan.FromSeconds(30));

        stopWatch.Stop();

        var elapsed = stopWatch.Elapsed;

        testOutputHelper.WriteLine("Took {0:g} for HomeAssistant MQTT Integration to come online", elapsed);
    }

    [Fact]
    public async Task TestCanExecuteTemplate()
    {
        await using var container = new HomeAssistantBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "H>"))
            .Build();

        await container.StartAsync();

        await container.PerformOnBoardingAsync();

        var result = await container.TemplateAsync("""
            {%- set greeting = 'Hello, World!' -%}
            {{ greeting }}
            """);

        result.Should().Be("Hello, World!");
    }

    [Fact]
    public async Task TestCanPerformOnBoarding()
    {
        await using var container = new HomeAssistantBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "H>"))
            .Build();

        await container.StartAsync();

        await container.PerformOnBoardingAsync();

        var client = container.WebUiHttpClient;

        (await client.GetAsync("/api/")).IsSuccessStatusCode.Should().Be(true);
    }

    [Fact]
    public async Task TestCanPerformOnBoardingTwice()
    {
        await using var container = new HomeAssistantBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "H>"))
            .Build();

        await container.StartAsync();

        await container.PerformOnBoardingAsync();

        var client = container.WebUiHttpClient;

        (await client.GetAsync("/api/")).IsSuccessStatusCode.Should().Be(true);

        await container.PerformOnBoardingAsync();

        (await client.GetAsync("/api/")).IsSuccessStatusCode.Should().Be(true);
    }

    [Fact]
    public async Task TestCanStart()
    {
        await using var container = new HomeAssistantBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "H>"))
            .Build();

        await container.StartAsync();

        var result = await container.CheckConfigAsync();

        testOutputHelper.WriteLine(result.Stderr);
        testOutputHelper.WriteLine(result.Stdout);

        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task TestFailureBeforeOnBoarding()
    {
        await using var container = new HomeAssistantBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "H>"))
            .Build();

        await container.StartAsync();

        var client = container.WebUiHttpClient;

        var result = await client.GetAsync("/api/");

        result.IsSuccessStatusCode.Should().Be(false);
    }
}
