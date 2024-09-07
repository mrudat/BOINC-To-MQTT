// <copyright file="MqttTestData.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Tests;

using System.Collections.Frozen;
using BOINC_To_MQTT.Tests.Scaffolding;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.Logging;
using Testcontainers;
using Testcontainers.EMQX;
using Testcontainers.HiveMQ;
using Testcontainers.Mosquitto;
using Xunit.Abstractions;

public static class MqttTestData
{
    private static readonly FrozenDictionary<string, MqttTestDatum> Data = new List<MqttTestDatum>
    {
        new MqttTestDatum<EmqxContainer, EmqxBuilder>(),
        new MqttTestDatum<HiveMQContainer, HiveMQBuilder>(),
        new MqttTestDatum<MosquittoContainer, MosquittoBuilder>(),
    }.ToFrozenDictionary(item => item.TypeName);

    public static IMqttContainer GetNewContainer(
        string containerTypeName,
        ILogger logger,
        ITestOutputHelper testOutputHelper,
        INetwork? network = null,
        string? networkAlias = null,
        string prefix = "")
    {
        return Data[containerTypeName]
            .GetContainer(
            logger,
            testOutputHelper,
            network,
            networkAlias,
            prefix);
    }

    public static TheoryData<string> Implementing<TInterface>() => TheoryDataStuff.Implementing<TInterface>(Data.Values.Select(item => item.Type));
}
