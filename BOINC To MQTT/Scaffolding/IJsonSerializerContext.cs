// <copyright file="IJsonSerializerContext.cs" company="Martin Rudat">
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
using System.Text.Json.Serialization;

/// <summary>
/// Indicates a class that provides support for compile-time JSON serialisation/deserialisation.
/// </summary>
/// <typeparam name="TJsonSerializerContext">The concrete <see cref="JsonSerializerContext"/> that is configured to serialize a type.</typeparam>
public interface IJsonSerializerContext<TJsonSerializerContext>
    where TJsonSerializerContext : JsonSerializerContext, IJsonSerializerContext<TJsonSerializerContext>
{
    /// <summary>
    /// Gets the default <typeparamref name="TJsonSerializerContext"/> associated with a default <see cref="JsonSerializerOptions"/> instance.
    /// </summary>
    public abstract static TJsonSerializerContext Default { get; }
}
