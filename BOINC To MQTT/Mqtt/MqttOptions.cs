// <copyright file="MqttOptions.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Mqtt;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Options for connecting to the MQTT server.
/// </summary>
public sealed class MqttOptions
{
    // TODO v3.1 clientId max length is 23, for V5 it's 256.
    //public const ushort MaximumClientIdentifierLength = 23;

    /// <summary>
    /// Maximum MQTT client identifier length (for v5.0).
    /// </summary>
    public const ushort MaximumClientIdentifierLength = 256;

    /// <summary>
    /// Maximum MQTT topic length.
    /// </summary>
    public const ushort MaximumTopicLength = ushort.MaxValue;

    /// <summary>
    /// Gets or sets a unique identifier for this host, defaults to host_cpid read from client_state.xml.
    /// </summary>
    [StringLength(maximumLength: MaximumClientIdentifierLength, MinimumLength = 1)]
    [MqttIdentifier]
    public string? ClientIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the discovery prefix for Home Assistant, needs to be set to a non-default value if the discovery prefix is changed in home assistant.
    /// </summary>
    [StringLength(maximumLength: MaximumTopicLength, MinimumLength = 1)]
    public string DiscoveryPrefix { get; set; } = "homeassistant";

    /// <summary>
    /// Gets or sets the host name of the MQTT server to connect to.
    /// </summary>
    [Required]
    public Uri Uri { get; set; } = new Uri("mqtt://localhost");
}
