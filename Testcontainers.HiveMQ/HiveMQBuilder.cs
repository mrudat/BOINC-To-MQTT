// <copyright file="HiveMQBuilder.cs" company="Martin Rudat">
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

namespace Testcontainers.HiveMQ;

using System.Text.RegularExpressions;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class HiveMQBuilder : ContainerBuilder<HiveMQBuilder, HiveMQContainer, HiveMQConfiguration>
{
    public const string HiveMQImage = "hivemq/hivemq4:4.28.0";

    public const string DefaultUserName = "username";

    public const ushort MqttPort = 1883;

    public const ushort MqttWebSocketsPort = 8000;

    public HiveMQBuilder()
        : this(new HiveMQConfiguration())
    {
        this.DockerResourceConfiguration = this.Init().DockerResourceConfiguration;
    }

    private HiveMQBuilder(HiveMQConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        this.DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override HiveMQConfiguration DockerResourceConfiguration { get; }

    /// <inheritdoc />
    public override HiveMQContainer Build()
    {
        this.Validate();

        var finalConfig = this;

        return new HiveMQContainer(finalConfig.DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override HiveMQBuilder Init()
    {
        return base.Init()
        .WithImage(HiveMQImage)
        .WithPortBinding(MqttPort, true)
        .WithPortBinding(MqttWebSocketsPort, true)
#if NET7_0_OR_GREATER
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(HiveMQIsRunning()));
#else
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(new Regex(@"Started HiveMQ")));
#endif
    }

#if NET7_0_OR_GREATER

    [GeneratedRegex(@"Started HiveMQ")]
    private static partial Regex HiveMQIsRunning();

#endif

    /// <inheritdoc />
    protected override void Validate() => base.Validate();

    /// <inheritdoc />
    protected override HiveMQBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return this.Merge(this.DockerResourceConfiguration, new HiveMQConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override HiveMQBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return this.Merge(this.DockerResourceConfiguration, new HiveMQConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override HiveMQBuilder Merge(HiveMQConfiguration oldValue, HiveMQConfiguration newValue)
    {
        return new HiveMQBuilder(new HiveMQConfiguration(oldValue, newValue));
    }
}
