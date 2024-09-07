// <copyright file="Boinc2MqttOptions.cs" company="Martin Rudat">
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
using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Mqtt;
using Microsoft.Extensions.Options;

/// <summary>
/// Options for the BOINC To MQTT program.
/// </summary>
public sealed partial class Boinc2MqttOptions
{
    /// <summary>
    /// Name of the configuration section.
    /// </summary>
    public const string ConfigurationSectionName = "BOINC2MQTT";

    /// <summary>
    /// Gets or sets the list of <see cref="LocalBoincOptions"/>.
    /// </summary>
    [ValidateEnumeratedItems]
    public HashSet<LocalBoincOptions> Local { get; set; } = [];

    /// <summary>
    /// Gets or sets the <see cref="MqttOptions"/>.
    /// </summary>
    [Required]
    [ValidateObjectMembers]
    public required MqttOptions MQTT { get; set; }

    /// <summary>
    /// Gets or sets the list of <see cref="RemoteBoincOptions"/>.
    /// </summary>
    [ValidateEnumeratedItems]
    public HashSet<RemoteBoincOptions> Remote { get; set; } = [];
}
