// <copyright file="IMqttTlsContainer.cs" company="Martin Rudat">
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

public interface IMqttTlsContainer : ICommonMqttContainer, IGetServerCertificate
{
    /// <summary>
    /// Gets the listening port for MQTT over TLS.
    /// </summary>
    public ushort MqttTlsPort { get; }

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for connecting to the MQTT server using MQTT with TLS.
    /// </summary>
    /// <param name="userName">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for connecting to the MQTT server.</returns>
    public Uri GetMqttTlsUri(string? userName = null);

    /// <inheritdoc/>
    Uri ICommonMqttContainer.GetMqttUri(string? userName) => this.GetMqttTlsUri(userName);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for other containers to connect to the MQTT server using MQTT with TLS.
    /// </summary>
    /// <param name="userName">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for other containers to connect to the MQTT server.</returns>
    public Uri GetNetworkMqttTlsUri(string? userName = null);

    /// <inheritdoc/>
    Uri ICommonMqttContainer.GetNetworkMqttUri(string? userName) => this.GetNetworkMqttTlsUri(userName);
}
