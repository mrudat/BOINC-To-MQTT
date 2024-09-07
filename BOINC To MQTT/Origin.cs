// <copyright file="Origin.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// A description of the software that is configuring this device.
/// From https://www.home-assistant.io/integrations/mqtt/
/// </summary>
public record Origin
{
    private Origin()
    {
    }

    private static readonly Lazy<Origin> Singleton = new(() => new Origin()
    {
        Name = "BOINC To MQTT",
        SoftwareVersion = ThisAssembly.Git.Commit,
        SupportUrl = ThisAssembly.Git.Url != string.Empty ? new Uri(ThisAssembly.Git.Url) : null,
    });

    /// <summary>
    /// Gets a properly-configured <see cref="Origin"/>.
    /// </summary>
    public static Origin Instance { get; } = Singleton.Value;

    /// <summary>
    /// Gets the name of the application that is the origin the discovered MQTT item.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonRequired]
    [Required]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the version of the software that supplies the discovered MQTT item.
    /// </summary>
    [JsonPropertyName("sw")]
    public string? SoftwareVersion { get; init; }

    /// <summary>
    /// Gets the support URL for the application that supplies the discovered MQTT item.
    /// </summary>
    [JsonPropertyName("url")]
    public Uri? SupportUrl { get; init; }
}
