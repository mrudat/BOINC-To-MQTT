// <copyright file="BoincBuilder.cs" company="Martin Rudat">
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

namespace TestContainers.BOINC;

using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class BoincBuilder : ContainerBuilder<BoincBuilder, BoincContainer, BoincConfiguration>
{
    public const string BOINCImage = "boinc/client:latest";

    public const ushort GuiRpcPort = 31416;

    public BoincBuilder()
        : this(new BoincConfiguration())
    {
        this.DockerResourceConfiguration = this.Init().DockerResourceConfiguration;
    }

    private BoincBuilder(BoincConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        this.DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override BoincConfiguration DockerResourceConfiguration { get; }

    /// <inheritdoc />
    public override BoincContainer Build()
    {
        this.Validate();
        return new BoincContainer(this.DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override BoincBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return this.Merge(this.DockerResourceConfiguration, new BoincConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override BoincBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return this.Merge(this.DockerResourceConfiguration, new BoincConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override BoincBuilder Init()
    {
        return base.Init()
            .WithImage(BOINCImage)
            .WithEnvironment("BOINC_CMD_LINE_OPTIONS", "--allow_remote_gui_rpc")
            .WithPortBinding(GuiRpcPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Initialization completed"));
    }

    /// <inheritdoc />
    protected override BoincBuilder Merge(BoincConfiguration oldValue, BoincConfiguration newValue)
    {
        return new BoincBuilder(new BoincConfiguration(oldValue, newValue));
    }

    /// <inheritdoc />
    protected override void Validate()
    {
        base.Validate();
    }
}
