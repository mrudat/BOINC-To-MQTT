// <copyright file="EmqxContainerTest.cs" company="Martin Rudat">
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

namespace Testcontainers.EMQX.Tests;

using Testcontainers.Tests;
using Xunit.Abstractions;

[Collection("Container")]
public class CommonEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : CommonMqttContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture)
{
}

[Collection("Container")]
public class MqttEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : MqttContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture)
{
}

[Collection("Container")]
public class MqttTlsEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : MqttTlsContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture)
{
}

[Collection("Container")]
public class WebSocketsEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : MqttWebSocketsContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture)
{
}

[Collection("Container")]
public class WebSocketsTlsEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : MqttWebSocketsTlsContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture)
{
}

#pragma warning disable S125 // Sections of code should not be commented out
// TODO implement IRequiresAuthentication
//[SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3844")]//[Collection("Container")]
//public class AddUserEmqxContainerTests(ITestOutputHelper testOutputHelper, EmqxFixture containerFixture) : AddUserContainerTests<EmqxContainer, EmqxBuilder>(testOutputHelper, containerFixture) { }
#pragma warning restore S125 // Sections of code should not be commented out

[CollectionDefinition("Container")]
public class ContainerCollection : ICollectionFixture<EmqxFixture>
{
}

public class EmqxFixture : ContainerFixture<EmqxContainer, EmqxBuilder>
{
}
