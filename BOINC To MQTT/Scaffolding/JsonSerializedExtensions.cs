// <copyright file="JsonSerializedExtensions.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Scaffolding;

using System.Text.Json;

/// <summary>
/// 
/// </summary>
public static class JsonSerializedExtensions
{
    /// <summary>
    /// Returns a <see cref="string"/> containing the JSON serialization of <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="TSerialized">The type to be serialized.</typeparam>
    /// <param name="value">The value to be serialized.</param>
    /// <returns>A <see cref="string"/> containing the JSON serialization of <paramref name="value"/>.</returns>
    public static string SerializeToJson<TSerialized>(this TSerialized value)
        where TSerialized : IJsonSerialized<TSerialized> => JsonSerializer.Serialize(value, ((IJsonSerialized<TSerialized>)value).GetTypeInfo());

    /// <summary>
    /// Returns a <see cref="byte[]"/> containing the JSON serialization of <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="TSerialized">The type to be serialized.</typeparam>
    /// <param name="value">The value to be serialized.</param>
    /// <returns>A <see cref="byte[]"/> containing the JSON serialization of <paramref name="value"/>.</returns>
    public static byte[] SerializeToJsonUtf8Bytes<TSerialized>(this TSerialized value)
            where TSerialized : IJsonSerialized<TSerialized> => JsonSerializer.SerializeToUtf8Bytes(value, ((IJsonSerialized<TSerialized>)value).GetTypeInfo());
}
