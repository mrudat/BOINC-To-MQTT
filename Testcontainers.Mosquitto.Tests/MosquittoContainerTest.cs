// <copyright file="MosquittoContainerTest.cs" company="Martin Rudat">
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

namespace Testcontainers.Mosquitto.Tests;

using Testcontainers.Tests;
using Xunit.Abstractions;

[Collection("Container")]
public class CommonMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : CommonMqttContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture)
{
}

[Collection("Container")]
public class MqttMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : MqttContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture)
{
}

#pragma warning disable S125 // Sections of code should not be commented out
/* TODO implement TLS
[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3844")]
[Collection("Container")]
public class MqttTlsMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : MqttTlsContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture) { }
*/
#pragma warning restore S125 // Sections of code should not be commented out

[Collection("Container")]
public class WebSocketsMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : MqttWebSocketsContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture)
{
}

#pragma warning disable S125 // Sections of code should not be commented out
/* TODO implement TLS
[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3844")]
[Collection("Container")]
public class WebSocketsTlsMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : MqttWebSocketsTlsContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture) { }
*/
#pragma warning restore S125 // Sections of code should not be commented out

[Collection("Container")]
public class AddUserMosquittoContainerTests(ITestOutputHelper testOutputHelper, MosquittoFixture containerFixture) : AddUserContainerTests<MosquittoContainer, MosquittoBuilder>(testOutputHelper, containerFixture)
{
}

[CollectionDefinition("Container")]
public class ContainerCollection : ICollectionFixture<MosquittoFixture>
{
}

public class MosquittoFixture : ContainerFixture<MosquittoContainer, MosquittoBuilder>
{
}
