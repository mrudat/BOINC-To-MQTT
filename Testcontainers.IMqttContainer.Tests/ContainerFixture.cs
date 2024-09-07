// <copyright file="ContainerFixture.cs" company="Martin Rudat">
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
using DotNet.Testcontainers.Networks;

public abstract class ContainerFixture<TContainer, TBuilder> : IAsyncLifetime
    where TContainer : class, ICommonMqttContainer
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    protected ContainerFixture()
    {
        // TODO logging?
        this.Container = new TBuilder()
            .Build();

        this.Network = new NetworkBuilder()
            .Build();

        this.ContainerOnNetwork = new TBuilder()
            .WithNetwork(this.Network)
            .WithNetworkAliases(nameof(TContainer))
            .Build();
    }

    public TContainer Container { get; }

    public TContainer ContainerOnNetwork { get; }

    public INetwork Network { get; }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await Task.WhenAll(
            this.Container.DisposeAsync().AsTask(),
            this.ContainerOnNetwork.DisposeAsync().AsTask());
        await this.Network.DisposeAsync();
    }

    Task IAsyncLifetime.InitializeAsync()
    {
        return Task.WhenAll(
            this.Container.StartAsync(),
            this.ContainerOnNetwork.StartAsync());
    }
}
