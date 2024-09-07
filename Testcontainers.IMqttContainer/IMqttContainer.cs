// <copyright file="IMqttContainer.cs" company="Martin Rudat">
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

namespace Testcontainers;

public interface IMqttContainer : ICommonMqttContainer
{
    /// <summary>
    /// Gets the listening port for unencrypted MQTT.
    /// </summary>
    public ushort MqttPort { get; }

    /// <inheritdoc/>
    Uri ICommonMqttContainer.GetMqttUri(string? userName) => this.GetMqttUri(userName);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for connecting to the MQTT server using MQTT.
    /// </summary>
    /// <param name="userName">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for connecting to the MQTT server using MQTT.</returns>
    public new Uri GetMqttUri(string? userName = null);

    /// <inheritdoc/>
    Uri ICommonMqttContainer.GetNetworkMqttUri(string? userName) => this.GetNetworkMqttUri(userName);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for other containers to connect to the MQTT server using MQTT.
    /// </summary>
    /// <param name="userName">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for other containers to connect to the MQTT server using MQTT.</returns>
    public new Uri GetNetworkMqttUri(string? userName = null);
}
