// <copyright file="BoincIntegrationTests.cs" company="Martin Rudat">
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
using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Tests.Scaffolding;
using BoincRpc;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers;
using Testcontainers.BOINC;
using Testcontainers.Mosquitto;
using Xunit.Abstractions;

public class BoincIntegrationTests(ITestOutputHelper testOutputHelper)
{
    private readonly ILogger<BoincIntegrationTests> logger = XUnitLogger.CreateLogger<BoincIntegrationTests>(testOutputHelper);

    [Fact]
    public async Task TestAuthenticationFailure()
    {
        // The MQTT container that starts fastest
        await using var mqttContainer = new MosquittoBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "M>"))
            .Build();

        await using var boincContainer = new BoincBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "B>"))
            .Build();

        var mqttContainerStarted = mqttContainer.StartAsync();

        var boincContainerStarted = boincContainer.StartAsync();

        var builder = Program.CreateApplicationBuilder([]);

        MockFileSystem mockFileSystem = new();

        mockFileSystem
            .AddDirectory(builder.Environment.ContentRootPath);

        builder.Services
            .AddSingleton<IFileSystem>(mockFileSystem);

        builder.Logging
            .AddProvider(new XUnitLoggerProvider(testOutputHelper));

        await Task.WhenAll(boincContainerStarted, mqttContainerStarted);

        builder.Configuration
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["BOINC2MQTT:MQTT:URI"] = ((IMqttContainer)mqttContainer).GetMqttUri().ToString(),
                ["BOINC2MQTT:Remote:0:BoincUri"] = new UriBuilder("tcp", boincContainer.Hostname, boincContainer.GuiRpcPort).ToString(),
                ["BOINC2MQTT:Remote:0:GuiRpcKey"] = "Some Invalid Value",
            });

        using var host = builder.Build();

        using CancellationTokenSource cts = new();

        cts.CancelAfter(TimeSpan.FromSeconds(5));

        await host.Invoking(c => c!.RunAsync(cts.Token))
            .Should()
            .ThrowAsync<AuthorisationFailedException>();
    }

    [Fact]
    public async Task TestCanReconnect()
    {
        await using var boincContainer = new BoincBuilder()
            .WithLogger(this.logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, "B>"))
            .Build();

        var boincContainerStarted = boincContainer.StartAsync();

        var builder = Program.CreateApplicationBuilder([]);

        MockFileSystem mockFileSystem = new();

        mockFileSystem
            .AddDirectory(builder.Environment.ContentRootPath);

        builder.Services
            .AddSingleton<IFileSystem>(mockFileSystem);

        builder.Logging
            .AddProvider(new XUnitLoggerProvider(testOutputHelper));

        using var host = builder.Build();

        using var scope = host.Services.CreateScope();

        var boincContext = (BoincContext)scope.ServiceProvider.GetRequiredService<IBoincContext>();

        await boincContainerStarted;

        boincContext.BoincOptions = new RemoteBoincOptions()
        {
            BoincUri = new UriBuilder("tcp", boincContainer.Hostname, boincContainer.GuiRpcPort).Uri,
            GuiRpcKey = await boincContainer.GetGuiRpcKeyAsync(),
        };

        var boincConnection = scope.ServiceProvider.GetRequiredService<IBoincConnection>();

        await boincConnection.Invoking(bc => bc.GetHostInfoAsync()).Should().CompleteWithinAsync(TimeSpan.FromSeconds(5));

        await boincContainer.StopAsync();

        using var cts = new CancellationTokenSource();

        cts.CancelAfter(TimeSpan.FromSeconds(5));

        await boincConnection.Invoking(bc => bc.GetHostInfoAsync(cts.Token)).Should().ThrowAsync<InvalidRpcResponseException>();

        await boincContainer.StartAsync();

        await boincConnection.Invoking(bc => bc.GetHostInfoAsync()).Should().CompleteWithinAsync(TimeSpan.FromSeconds(5));
    }
}
