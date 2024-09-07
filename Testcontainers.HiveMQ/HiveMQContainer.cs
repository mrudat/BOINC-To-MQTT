// <copyright file="HiveMQContainer.cs" company="Martin Rudat">
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

using DotNet.Testcontainers.Containers;

public class HiveMQContainer(HiveMQConfiguration configuration) : DockerContainer(configuration), IMqttContainer, IMqttWebSocketsContainer
{
    ushort IMqttContainer.MqttPort => this.GetMappedPublicPort(HiveMQBuilder.MqttPort);

    ushort IMqttWebSocketsContainer.MqttWebSocketsPort => this.GetMappedPublicPort(HiveMQBuilder.MqttWebSocketsPort);

    Uri ICommonMqttContainer.GetMqttUri(string? userName) => (this as IMqttContainer).GetMqttUri(userName);

    Uri IMqttContainer.GetMqttUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, this.Hostname, (this as IMqttContainer).MqttPort), userName).Uri;

    Uri ICommonMqttContainer.GetNetworkMqttUri(string? userName) => (this as IMqttContainer).GetNetworkMqttUri(userName);

    Uri IMqttContainer.GetNetworkMqttUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, configuration.NetworkAliases.First(), HiveMQBuilder.MqttPort), userName).Uri;

    Uri IMqttWebSocketsContainer.GetNetworkWebSocketsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWs, configuration.NetworkAliases.First(), HiveMQBuilder.MqttWebSocketsPort, "mqtt"), userName).Uri;

    Uri IMqttWebSocketsContainer.GetWebSocketsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWs, this.Hostname, (this as IMqttWebSocketsContainer).MqttWebSocketsPort, "mqtt"), userName).Uri;

    private static UriBuilder SetCredentials(UriBuilder uriBuilder, string? userName)
    {
        uriBuilder.UserName = Uri.EscapeDataString(userName ?? HiveMQBuilder.DefaultUserName);
        return uriBuilder;
    }
}
