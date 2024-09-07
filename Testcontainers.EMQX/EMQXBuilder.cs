// <copyright file="EmqxBuilder.cs" company="Martin Rudat">
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

namespace Testcontainers.EMQX;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class EmqxBuilder : ContainerBuilder<EmqxBuilder, EmqxContainer, EmqxConfiguration>
{
    public const ushort DashboardPort = 18083;
    public const string DefaultUserName = "username";
    public const string EmqxImage = "emqx/emqx:5.6.0";

    [StringSyntax("Regex")]
    public const string EMQXIsRunningPattern = @"EMQX \S+ is running now!";

    public const ushort MqttPort = 1883;

    public const ushort MqttTlsPort = 8883;

    public const ushort MqttWebSocketsPort = 8083;

    public const ushort MqttWebSocketsTlsPort = 8084;

    private static readonly Regex EMQXIsRunningRegex = MakeEMQXIsRunning();

    public EmqxBuilder()
        : this(new EmqxConfiguration())
    {
        this.DockerResourceConfiguration = this.Init().DockerResourceConfiguration;
    }

    private EmqxBuilder(EmqxConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        this.DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override EmqxConfiguration DockerResourceConfiguration { get; }

    /// <inheritdoc />
    public override EmqxContainer Build()
    {
        this.Validate();
        return new EmqxContainer(this.DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override EmqxBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return this.Merge(this.DockerResourceConfiguration, new EmqxConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override EmqxBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return this.Merge(this.DockerResourceConfiguration, new EmqxConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override EmqxBuilder Init()
    {
        return base.Init()
            .WithImage(EmqxImage)
            .WithPortBinding(MqttPort, true)
            .WithPortBinding(MqttTlsPort, true)
            .WithPortBinding(MqttWebSocketsPort, true)
            .WithPortBinding(MqttWebSocketsTlsPort, true)
            .WithPortBinding(DashboardPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(EMQXIsRunningRegex));
    }

    /// <inheritdoc />
    protected override EmqxBuilder Merge(EmqxConfiguration oldValue, EmqxConfiguration newValue)
    {
        return new EmqxBuilder(new EmqxConfiguration(oldValue, newValue));
    }

    /// <inheritdoc />
    protected override void Validate() => base.Validate();

#if NET7_0_OR_GREATER
    [GeneratedRegex(EMQXIsRunningPattern)]
    private static partial Regex MakeEMQXIsRunning();
#else
    private static Regex MakeEMQXIsRunning() => new(EMQXIsRunningPattern);
#endif
}
