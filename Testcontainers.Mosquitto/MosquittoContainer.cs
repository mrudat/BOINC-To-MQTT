// <copyright file="MosquittoContainer.cs" company="Martin Rudat">
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

using DotNet.Testcontainers.Containers;

/// <summary>
/// An Apache Mosquitto container instance.
/// </summary>
/// <param name="configuration">A <see cref="MosquittoConfiguration"/> that provides the configuration for this Apache Mosquitto container instance.</param>
public class MosquittoContainer(MosquittoConfiguration configuration) : DockerContainer(configuration), IMqttContainer, IMqttWebSocketsContainer, IRequiresAuthentication
{
    /// <inheritdoc/>
    ushort IMqttContainer.MqttPort => this.GetMappedPublicPort(MosquittoBuilder.MqttPort);

    /// <inheritdoc/>
    ushort IMqttWebSocketsContainer.MqttWebSocketsPort => this.GetMappedPublicPort(MosquittoBuilder.MqttWebSocketsPort);

    /// <inheritdoc/>
    async Task IRequiresAuthentication.AddUser(string userName, string password, CancellationToken cancellationToken)
    {
        async Task Exec(string[] command)
        {
            var result = await this.ExecAsync(command, cancellationToken).ConfigureAwait(false);

            if (result.ExitCode != 0)
            {
                throw new Exception("TODO throw a better exception");
            }
        }

        await Exec(["/usr/bin/mosquitto_passwd", "-b", "/mosquitto/config/passwd", userName, password]).ConfigureAwait(false);

        configuration.Users[userName] = password;

        await Exec(["kill", "-HUP", "1"]).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    Uri ICommonMqttContainer.GetMqttUri(string? userName) => (this as IMqttContainer).GetMqttUri(userName);

    /// <inheritdoc/>
    Uri IMqttContainer.GetMqttUri(string? userName) => this.SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, this.Hostname, (this as IMqttContainer).MqttPort), userName).Uri;

    /// <inheritdoc/>
    Uri ICommonMqttContainer.GetNetworkMqttUri(string? userName) => (this as IMqttContainer).GetNetworkMqttUri(userName);

    /// <inheritdoc/>
    Uri IMqttContainer.GetNetworkMqttUri(string? userName) => this.SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, configuration.NetworkAliases.First(), MosquittoBuilder.MqttPort), userName).Uri;

    /// <inheritdoc/>
    Uri IMqttWebSocketsContainer.GetNetworkWebSocketsUri(string? userName) => this.SetCredentials(new UriBuilder(Uri.UriSchemeWs, configuration.NetworkAliases.First(), MosquittoBuilder.MqttWebSocketsPort), userName).Uri;

    /// <inheritdoc/>
    Uri IMqttWebSocketsContainer.GetWebSocketsUri(string? userName) => this.SetCredentials(new UriBuilder(Uri.UriSchemeWs, this.Hostname, (this as IMqttWebSocketsContainer).MqttWebSocketsPort), userName).Uri;

    private UriBuilder SetCredentials(UriBuilder uriBuilder, string? username)
    {
        if (username == null)
        {
            uriBuilder.UserName = Uri.EscapeDataString(configuration.UserName!);
            uriBuilder.Password = Uri.EscapeDataString(configuration.Password!);
        }
        else
        {
            uriBuilder.UserName = Uri.EscapeDataString(username);
            if (configuration.Users.TryGetValue(username, out var password))
            {
                uriBuilder.Password = Uri.EscapeDataString(password);
            }
        }

        return uriBuilder;
    }
}
