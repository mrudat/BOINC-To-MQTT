// <copyright file="MqttTopicAttribute.cs" company="Martin Rudat">
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
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

internal partial class MqttTopicAttribute : RegularExpressionAttribute
{
    [StringSyntax("Regex")]
    private const string ValidTopicPattern = @"[^$+#\0][^+#\0]*";

    public MqttTopicAttribute()
        : base(ValidTopicPattern)
    {
    }

    [GeneratedRegex(ValidTopicPattern, RegexOptions.Singleline)]
    public partial Regex ValidTopicRegex();
}
