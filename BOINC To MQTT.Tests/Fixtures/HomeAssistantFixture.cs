// <copyright file="HomeAssistantFixture.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Tests.Fixtures;

using DotNet.Testcontainers.Builders;
using Testcontainers;
using Testcontainers.HomeAssistant;

[Collection(nameof(MqttCollection))]
public class HomeAssistantFixture<TMqttContainer, TMqttBuilder>(MqttFixture<TMqttContainer, TMqttBuilder> mqttFixture) : IAsyncLifetime, IHomeAssistantFixture
    where TMqttContainer : class, IMqttContainer
    where TMqttBuilder : class, IContainerBuilder<TMqttBuilder, TMqttContainer>, new()
{
    public const string MqttPassword = "homeassistant";
    public const string MqttUserName = "homeassistant";
    private HomeAssistantContainer? container;

    public HomeAssistantContainer Container => this.container!;

    IMqttContainer IHomeAssistantFixture.MqttContainer => mqttFixture.Container;

    Task IAsyncLifetime.DisposeAsync() => this.container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    async Task IAsyncLifetime.InitializeAsync()
    {
        using CancellationTokenSource cancellationTokenSource = new();

        var cancellationToken = cancellationTokenSource.Token;

        this.container = new HomeAssistantBuilder()
            //.WithLogger(logger)
            .Build();

        await this.container.StartAsync(cancellationToken);

        await this.container.PerformOnBoardingAsync(cancellationToken);

        if (mqttFixture.Container is IRequiresAuthentication addUser)
        {
            await addUser.AddUser(MqttUserName, MqttPassword, cancellationToken);
        }

        await this.container.ConfigureMqttAsync(mqttFixture.Container, MqttUserName, cancellationToken);
    }
}
