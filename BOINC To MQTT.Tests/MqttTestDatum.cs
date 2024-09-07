// <copyright file="MqttTestDatum.cs" company="Martin Rudat">
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

using BOINC_To_MQTT.Tests.Scaffolding;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.Logging;
using Testcontainers;
using Xunit.Abstractions;

public abstract record MqttTestDatum(Type Type, string TypeName)
{
    public abstract IMqttContainer GetContainer(ILogger logger, ITestOutputHelper testOutputHelper, INetwork? network, string? networkAlias, string prefix);
}

public record MqttTestDatum<TContainer, TBuilder>() : MqttTestDatum(typeof(TContainer), /* Important! not nameof(TContainer) */ typeof(TContainer).Name)
    where TContainer : class, IMqttContainer
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    /// <inheritdoc/>
    public override IMqttContainer GetContainer(
        ILogger logger,
        ITestOutputHelper testOutputHelper,
        INetwork? network,
        string? networkAlias,
        string prefix)
    {
        var builder = new TBuilder()
            .WithLogger(logger)
            .WithOutputConsumer(new TestOutputHelperOutputConsumer(testOutputHelper, prefix));

        // TODO surely there's got to be a better way?
        if (network != null)
        {
            builder = builder.WithNetwork(network);
        }

        if (networkAlias != null)
        {
            builder = builder.WithNetworkAliases(networkAlias);
        }

        return builder.Build();
    }
}
