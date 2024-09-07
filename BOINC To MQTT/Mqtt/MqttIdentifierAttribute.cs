// <copyright file="MqttIdentifierAttribute.cs" company="Martin Rudat">
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
/// Specifies that a data field must be a valid MQTT identifier.<br />
/// Must consist of characters from the character class [a-zA-Z0-9_-] (alphanumerics, underscore and hyphen).
/// </summary>
#if true // TODO https://github.com/dotnet/runtime/issues/101965
public partial class MqttIdentifierAttribute() : RegularExpressionAttribute(@"^[a-zA-Z0-9_-]+$")
{
}
#else
public partial class MqttIdentifierAttribute() : RegularExpressionAttribute(MqttIdentifierRegex)
{
    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    private static partial Regex MqttIdentifierRegex();
}
#endif
