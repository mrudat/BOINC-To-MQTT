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

namespace Testcontainers.HomeAssistant;

using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

public interface IJsonSerialized<TObject>
    where TObject : IJsonSerialized<TObject>
{
    public JsonTypeInfo<TObject> GetTypeInfo();
}

public interface IJsonSerialized<TObject, TSerializerContext> : IJsonSerialized<TObject>
    where TObject : IJsonSerialized<TObject, TSerializerContext>
    where TSerializerContext : JsonSerializerContext, IJsonSerializerContext<TSerializerContext>, new()
{
    JsonTypeInfo<TObject> IJsonSerialized<TObject>.GetTypeInfo() => TSerializerContext.Default.GetTypeInfo(typeof(TObject)) as JsonTypeInfo<TObject> ?? throw new InvalidOperationException($"{nameof(TSerializerContext)} does not know how to serialize {nameof(TObject)}");
}
