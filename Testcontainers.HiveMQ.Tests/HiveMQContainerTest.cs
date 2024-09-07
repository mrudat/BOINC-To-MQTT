// <copyright file="HiveMQContainerTest.cs" company="Martin Rudat">
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

namespace Testcontainers.HiveMQ.Tests;

using Testcontainers.Tests;
using Xunit.Abstractions;

[Collection("Container")]
public class CommonHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : CommonMqttContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture)
{
}

[Collection("Container")]
public class MqttHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : MqttContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture)
{
}

#pragma warning disable S125 // Sections of code should not be commented out
/* TODO Implement Tls
[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3844")]
[Collection("Container")]
public class MqttTlsHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : MqttTlsContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture) { }
*/

[Collection("Container")]
public class WebSocketsHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : MqttWebSocketsContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture)
{
}

/* TODO Implement Tls
[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3844")]
[Collection("Container")]
public class WebSocketsTlsHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : MqttWebSocketsTlsContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture) { }
*/

/* TODO implement IRequiresAuthentication
[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3844")]
[Collection("Container")]
public class AddUserHiveMQContainerTests(ITestOutputHelper testOutputHelper, HiveMQFixture containerFixture) : AddUserContainerTests<HiveMQContainer, HiveMQBuilder>(testOutputHelper, containerFixture) { }
*/
#pragma warning restore S125 // Sections of code should not be commented out

[CollectionDefinition("Container")]
public class ContainerCollection : ICollectionFixture<HiveMQFixture>
{
}

public class HiveMQFixture : ContainerFixture<HiveMQContainer, HiveMQBuilder>
{
}
