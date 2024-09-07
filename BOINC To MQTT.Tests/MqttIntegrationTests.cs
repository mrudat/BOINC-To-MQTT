// <copyright file="MqttIntegrationTests.cs" company="Martin Rudat">
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
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Adapter;
using Testcontainers;
using Xunit.Abstractions;

public class MqttIntegrationTests(ITestOutputHelper testOutputHelper)
{
    public static readonly TheoryData<string> RequiresAuthentication = MqttTestData.Implementing<IRequiresAuthentication>();
    protected readonly ILogger<MqttIntegrationTests> logger = XUnitLogger.CreateLogger<MqttIntegrationTests>(testOutputHelper);

    [Theory]
    [MemberData(nameof(RequiresAuthentication))]
    public async Task TestMqttAuthenticationFailure(string containerTypeName)
    {
        await using ICommonMqttContainer mqttContainer = MqttTestData.GetNewContainer(containerTypeName, this.logger, testOutputHelper, prefix: "M>");

        var containerStarted = mqttContainer.StartAsync();

        var builder = Program.CreateApplicationBuilder([]);

        MockFileSystem mockFileSystem = new();

        mockFileSystem
            .AddDirectory(builder.Environment.ContentRootPath);

        builder.Services
            .AddSingleton<IFileSystem>(mockFileSystem);

        builder.Logging
            .AddProvider(new XUnitLoggerProvider(testOutputHelper));

        await containerStarted;

        builder.Configuration
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["BOINC2MQTT:MQTT:URI"] = new UriBuilder(mqttContainer.GetMqttUri("unknownUser")).ToString(),
            });

        using var host = builder.Build();

        using CancellationTokenSource cts = new();

        cts.CancelAfter(TimeSpan.FromSeconds(5));

        await host.Invoking(c => c!.RunAsync(cts.Token))
            .Should()
            .ThrowAsync<MqttConnectingFailedException>();
    }
}
