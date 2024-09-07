// <copyright file="OnBoardingIntegrationData.cs" company="Martin Rudat">
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

/// <summary>
/// Data to be supplied to the on-boarding integration step.
/// </summary>
public class OnBoardingIntegrationData : IJsonSerialized<OnBoardingIntegrationData, SourceGenerationContext>
{
    /// <summary>
    /// Gets the Client Identifier, usually the <see cref="Uri"/> of the client web-page.
    /// </summary>
    [JsonPropertyName("client_id")]
    [JsonRequired]
    [Required]
    public Uri ClientIdentifier { get; init; }

    /// <summary>
    /// Gets the RediectUri, usually a <see cref="Uri"/> handled by the client web-page.
    /// </summary>
    [JsonPropertyName("redirect_uri")]
    [JsonRequired]
    [Required]
    public Uri RedirectUri { get; init; }
}
