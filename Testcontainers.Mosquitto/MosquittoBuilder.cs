// <copyright file="MosquittoBuilder.cs" company="Martin Rudat">
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

namespace Testcontainers.Mosquitto;

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Docker.DotNet.Models;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class MosquittoBuilder : ContainerBuilder<MosquittoBuilder, MosquittoContainer, MosquittoConfiguration>
{
    public const string DefaultPassword = "password";
    public const string DefaultUserName = "username";

    [StringSyntax("Regex")]
    public const string MosquittoIsRunningPattern = @"mosquitto version \S+ running";

    public const string MosquittoImage = "eclipse-mosquitto:2.0.18-openssl";
    public const ushort MqttPort = 1883;

    public const ushort MqttWebSocketsPort = 8080;

    private static readonly byte[] MosquittoConfFile = Encoding.UTF8.GetBytes($"""
        per_listener_settings true

        listener {MqttPort}
        protocol mqtt
        password_file /mosquitto/config/passwd

        listener {MqttWebSocketsPort}
        protocol websockets
        password_file /mosquitto/config/passwd
        """);

    private static readonly Regex MosquittoIsRunningRegex = MakeMosquittoIsRunning();

    public MosquittoBuilder()
        : this(new MosquittoConfiguration())
    {
        this.DockerResourceConfiguration = this.Init().DockerResourceConfiguration;
    }

    private MosquittoBuilder(MosquittoConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        this.DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override MosquittoConfiguration DockerResourceConfiguration { get; }

    /// <inheritdoc />
    public override MosquittoContainer Build()
    {
        this.Validate();

        return new MosquittoContainer(this.DockerResourceConfiguration);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="password"></param>
    /// <returns>A configured instance of <see cref="MosquittoBuilder"/>.</returns>
    public MosquittoBuilder WithPassword(string password)
    {
        return this.Merge(this.DockerResourceConfiguration, new MosquittoConfiguration(password: password));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="userName"></param>
    /// <returns>A configured instance of <see cref="MosquittoBuilder"/>.</returns>
    public MosquittoBuilder WithUserName(string userName)
    {
        return this.Merge(this.DockerResourceConfiguration, new MosquittoConfiguration(userName: userName));
    }

    /// <inheritdoc />
    protected override MosquittoBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return this.Merge(this.DockerResourceConfiguration, new MosquittoConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override MosquittoBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return this.Merge(this.DockerResourceConfiguration, new MosquittoConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override MosquittoBuilder Init()
    {
        return base.Init()
            .WithImage(MosquittoImage)
            .WithPortBinding(MqttPort, true)
            .WithPortBinding(MqttWebSocketsPort, true)
            .WithResourceMapping(Array.Empty<byte>(), "/mosquitto/config/passwd", UnixFileModes.UserRead | UnixFileModes.UserWrite)
            .WithResourceMapping(MosquittoConfFile, "/mosquitto/config/mosquitto.conf", UnixFileModes.UserRead | UnixFileModes.UserWrite)
            .WithUserName(DefaultUserName)
            .WithPassword(DefaultPassword)
            .WithStartupCallback(this.StartupCallback)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(MosquittoIsRunningRegex));
    }

    /// <inheritdoc />
    protected override MosquittoBuilder Merge(MosquittoConfiguration oldValue, MosquittoConfiguration newValue)
    {
        return new MosquittoBuilder(new MosquittoConfiguration(oldValue, newValue));
    }

    /// <inheritdoc />
    protected override void Validate()
    {
        base.Validate();

        _ = Guard.Argument(this.DockerResourceConfiguration.UserName, nameof(this.DockerResourceConfiguration.UserName))
          .NotNull()
          .NotEmpty();

        _ = Guard.Argument(this.DockerResourceConfiguration.Password, nameof(this.DockerResourceConfiguration.Password))
          .NotNull()
          .NotEmpty();
    }

#if NET7_0_OR_GREATER
    [GeneratedRegex(MosquittoIsRunningPattern)]
    private static partial Regex MakeMosquittoIsRunning();
#else
    private static Regex MakeMosquittoIsRunning() => new (MosquittoIsRunningPattern);
#endif

    private async Task StartupCallback(MosquittoContainer container, CancellationToken token)
    {
        await (container as IRequiresAuthentication).AddUser(this.DockerResourceConfiguration.UserName!, this.DockerResourceConfiguration.Password!, token).ConfigureAwait(false);
    }
}
