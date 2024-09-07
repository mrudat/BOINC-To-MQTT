// <copyright file="MqttFixture.cs" company="Martin Rudat">
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

public class MqttFixture<TContainer, TBuilder> : IMqttFixture, IAsyncLifetime
    where TContainer : class, IMqttContainer
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    public readonly TContainer Container = new TBuilder()
            // TODO capture container output.
            //.WithLogger(logger)
            .Build();

    public ICommonMqttContainer MqttContainer => this.Container;

    Task IAsyncLifetime.DisposeAsync() => this.Container.DisposeAsync().AsTask();

    Task IAsyncLifetime.InitializeAsync()
    {
        return this.Container.StartAsync();
    }
}
