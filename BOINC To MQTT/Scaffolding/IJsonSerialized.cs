// <copyright file="IJsonSerialized.cs" company="Martin Rudat">
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
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Indicates that <typeparamref name="TSerialized"/> supports compile-time JSON serialisation/deserialisation.
/// </summary>
/// <typeparam name="TSerialized">The type to be serialized.</typeparam>
public interface IJsonSerialized<TSerialized>
    where TSerialized : IJsonSerialized<TSerialized>
{
    /// <summary>
    /// Gets the <see cref="JsonTypeInfo{TObject}"/> for <typeparamref name="TSerialized"/> from the default <see cref="JsonSerializerContext"/> associated with a default <see cref="JsonSerializerOptions"/> instance.
    /// </summary>
    /// <returns>The <see cref="JsonTypeInfo{TObject}"/> for <typeparamref name="TSerialized"/> from the default <see cref="JsonSerializerContext"/> associated with a default <see cref="JsonSerializerOptions"/> instance.</returns>
    public JsonTypeInfo<TSerialized> GetTypeInfo();
}

/// <summary>
/// Indicates that <typeparamref name="TSerialized"/> supports compile-time JSON serialisation/deserialisation via <typeparamref name="TJsonSerializerContext"/>.
/// </summary>
/// <typeparam name="TSerialized">The type to be serialized.</typeparam>
/// <typeparam name="TJsonSerializerContext">The concrete <see cref="JsonSerializerContext"/> that is configured to serialize <typeparamref name="TSerialized"/>.</typeparam>
public interface IJsonSerialized<TSerialized, TJsonSerializerContext> : IJsonSerialized<TSerialized>
    where TSerialized : IJsonSerialized<TSerialized, TJsonSerializerContext>
    where TJsonSerializerContext : JsonSerializerContext, IJsonSerializerContext<TJsonSerializerContext>, new()
{
    /// <summary>
    /// Gets the <see cref="JsonTypeInfo{TObject}"/> for <typeparamref name="TSerialized"/> from the default <typeparamref name="TJsonSerializerContext"/> associated with a default <see cref="JsonSerializerOptions"/> instance.
    /// </summary>
    /// <returns>The <see cref="JsonTypeInfo{TObject}"/> for <typeparamref name="TSerialized"/> from the default <typeparamref name="TJsonSerializerContext"/> associated with a default <see cref="JsonSerializerOptions"/> instance.</returns>
    /// <exception cref="InvalidOperationException"><typeparamref name="TJsonSerializerContext"/> does not know how to serialise <typeparamref name="TSerialized"/>.</exception>
    JsonTypeInfo<TSerialized> IJsonSerialized<TSerialized>.GetTypeInfo() => TJsonSerializerContext.Default.GetTypeInfo(typeof(TSerialized)) as JsonTypeInfo<TSerialized> ?? throw new InvalidOperationException($"{nameof(TJsonSerializerContext)} does not know how to serialize {nameof(TSerialized)}, did you forget to add the [JsonSerializable(typeof({nameof(TSerialized)}))] attribute to {nameof(TJsonSerializerContext)}?");
}
