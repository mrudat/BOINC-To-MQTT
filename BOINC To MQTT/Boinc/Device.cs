// <copyright file="Device.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Boinc;

using System.Text.Json.Serialization;

/// <summary>
/// Information about a device to tie it into the device registry.<br />
/// From https://www.home-assistant.io/integrations/mqtt/.
/// </summary>
public record Device
{
    /// <summary>
    /// Gets a link to the web page that can manage the configuration of this device.
    /// Can be either an http://, https:// or an internal homeassistant:// URL.
    /// </summary>
    [JsonPropertyName("cu")]
    public Uri? ConfigurationUrl { get; init; }

    /// <summary>
    /// Gets the name of the device.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the firmware version of the device.
    /// </summary>
    [JsonPropertyName("sw")]
    public string? SoftwareVersion { get; init; }

    /// <summary>
    /// Gets a list of IDs that uniquely identify the device. For example a serial number.
    /// </summary>
    [JsonPropertyName("ids")]
    public List<string>? Identifiers { get; init; }

    /// <summary>
    /// Gets the manufacturer of the device.
    /// </summary>
    // TODO JsonPropertyName
    public string? Manufacturer { get; init; }

    /// <summary>
    /// Gets identifier of a device that routes messages between this device and Home Assistant.
    /// Examples of such devices are hubs, or parent devices of a sub-device.
    /// This is used to show device topology in Home Assistant.
    /// </summary>
    // TODO JsonPropertyName
    public string? ViaDevice { get; init; }
}
