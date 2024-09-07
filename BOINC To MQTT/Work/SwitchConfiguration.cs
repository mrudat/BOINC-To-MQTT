// <copyright file="SwitchConfiguration.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Work;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Scaffolding;
using MQTTnet.Protocol;

/// <summary>
/// Configuration variables for a MQTT Switch.<br />
/// From https://www.home-assistant.io/integrations/switch.mqtt/
/// and https://www.home-assistant.io/integrations/mqtt/
/// </summary>
public record SwitchConfiguration : IJsonSerialized<SwitchConfiguration, SourceGenerationContext>
{
    /// <summary>
    /// Gets the MQTT topic subscribed to receive availability (online/offline) updates.
    /// </summary>
    [JsonPropertyName("avty_t")]
    [MqttTopic]
    public string? AvailabilityTopic { get; init; }

    /// <summary>
    /// Gets the MQTT topic to publish commands to change the switch state.
    /// </summary>
    [JsonPropertyName("cmd_t")]
    [JsonRequired]
    [Required]
    [MqttTopic]
    public required string CommandTopic { get; init; }

    /// <summary>
    /// Gets information about the device this switch is a part of to tie it into the device registry.
    /// Only works when unique_id is set.
    /// At least one of identifiers or connections must be present to identify the device.
    /// </summary>
    [JsonPropertyName("dev")]
    public Device? Device { get; init; }

    /// <summary>
    /// Gets the MQTT topic subscribed to receive state updates.
    /// </summary>
    [JsonPropertyName("stat_t")]
    [MqttTopic]
    public string? StateTopic { get; init; }

    /// <summary>
    /// Gets an ID that uniquely identifies this switch device.
    /// If two switches have the same unique ID, Home Assistant will raise an exception.
    /// </summary>
    [JsonPropertyName("uniq_id")]
    public string? UniqueIdentifier { get; init; }

    /// <summary>
    /// Gets the name to use when displaying this switch.
    /// Default if not set is "MQTT Switch".<br />
    /// In the emitted JSON, can be set to null if only the device name is relevant, but the serialiser isn't configured to do that.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets if the published message should have the retain flag on or not.
    /// </summary>
    [JsonPropertyName("ret")]
    public bool? Retain { get; init; }

    /// <summary>
    /// Gets the maximum QoS level to be used when receiving and publishing messages.
    /// </summary>
    [JsonPropertyName("qos")]
    public MqttQualityOfServiceLevel? QualityOfService { get; init; }

    /// <summary>
    /// Gets a prefix/suffix that can be used to shorten topic names.
    /// A "~" at the start or the end of the other topic fields with be replaced with this value.
    /// </summary>
    [JsonPropertyName("~")]
    [MqttTopic]
    public string? TopicPrefix { get; init; }

    /// <summary>
    /// Gets information about the origin of this configuration.
    /// </summary>
    [JsonPropertyName("o")]
    public Origin? Origin { get; init; }
}
