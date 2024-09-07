// <copyright file="AbstractMqttContainerTests.cs" company="Martin Rudat">
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

namespace Testcontainers.Tests;

using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Xunit.Abstractions;

public abstract partial class AbstractMqttContainerTests<TContainer, TBuilder, TInterface>(ITestOutputHelper testOutputHelper, ContainerFixture<TContainer, TBuilder> containerFixture)
    where TContainer : class, ICommonMqttContainer, TInterface
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    protected readonly ILogger<AbstractMqttContainerTests<TContainer, TBuilder, TInterface>> logger = XUnitLogger.CreateLogger<AbstractMqttContainerTests<TContainer, TBuilder, TInterface>>(testOutputHelper);

    protected readonly MqttFactory mqttFactory = new();

    protected TContainer Container => containerFixture.Container;

    protected TContainer ContainerOnNetwork => containerFixture.ContainerOnNetwork;

    protected async Task AbstractTestCanConnectAsync(TInterface container)
    {
        Uri uri = this.GetUri(container);

        using var mqttClient = await this.ConnectTo(uri);

        mqttClient.IsConnected.Should().BeTrue();
    }

    protected void AbstractTestGetNetworkUriFails(TInterface container)
    {
        Action action = () =>
        {
            Uri uri = this.GetNetworkUri(container);
        };

        action.Should().Throw<InvalidOperationException>();
    }

    protected void AbstractTestGetNetworkUri(TInterface containerOnNetwork)
    {
        Uri uri = this.GetNetworkUri(containerOnNetwork);

        // TODO check that a mqtt client can actually connect to uri?
    }

    protected abstract Uri GetUri(TInterface container, string? name = null);

    protected abstract Uri GetNetworkUri(TInterface container, string? name = null);

    protected async Task<IMqttClient> ConnectTo(Uri uri)
    {
        var mqttClient = this.mqttFactory.CreateMqttClient();

        var mqttClientOptionsBuilder = this.mqttFactory.CreateClientOptionsBuilder();
        mqttClientOptionsBuilder
            .WithConnectionUri(uri);
        var usingTls = uri.Scheme.ToLowerInvariant() switch
        {
            "mqtts" or "wss" => true,
            _ => false,
        };

        if (usingTls)
        {
            // TODO fetch certificate to compare
            //var cert = await (container as IMqttTlsContainer)?.GetServerCertificateAsync();

            mqttClientOptionsBuilder
                .WithTlsOptions((opt) =>
                {
                    opt.WithCertificateValidationHandler((args) =>
                    {
                        //return args.Certificate.Equals(cert);
                        return true;
                    });
                });
        }

        var mqttClientOptions = mqttClientOptionsBuilder
            .Build();

        this.LogConnecting(uri);

        await mqttClient.ConnectAsync(mqttClientOptions);

        return mqttClient;
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Connecting to {uri}")]
    private partial void LogConnecting(Uri uri);
}
