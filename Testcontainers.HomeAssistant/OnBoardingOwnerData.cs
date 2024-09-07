// <copyright file="OnBoardingOwnerData.cs" company="Martin Rudat">
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

namespace Testcontainers.HomeAssistant;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public record OnBoardingOwnerData : IJsonSerialized<OnBoardingOwnerData, SourceGenerationContext>
{
    [JsonPropertyName("client_id")]
    [JsonRequired]
    [Required]
    public required Uri ClientIdentifier { get; init; }

    [JsonPropertyName("name")]
    [JsonRequired]
    [Required]
    public required string Name { get; init; }

    [JsonPropertyName("username")]
    [JsonRequired]
    [Required]
    public required string UserName { get; init; }

    [JsonPropertyName("password")]
    [JsonRequired]
    [Required]
    public required string Password { get; init; }

    [JsonPropertyName("language")]
    [JsonRequired]
    [Required]
    public required string Language { get; init; }
}
