// <copyright file="MqttWebSocketsTlsContainerTests.cs" company="Martin Rudat">
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

namespace Testcontainers.Tests;

using DotNet.Testcontainers.Builders;
using Xunit.Abstractions;

[Collection("Container")]
public abstract class MqttWebSocketsTlsContainerTests<TContainer, TBuilder>(ITestOutputHelper testOutputHelper, ContainerFixture<TContainer, TBuilder> containerFixture) : AbstractMqttContainerTests<TContainer, TBuilder, IMqttWebSocketsTlsContainer>(testOutputHelper, containerFixture)
    where TContainer : class, IMqttWebSocketsTlsContainer
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    [Fact]
    public Task TestCanConnectAsync() => this.AbstractTestCanConnectAsync(this.Container);

    [Fact]
    public void TestGetNetworkUri() => this.AbstractTestGetNetworkUri(this.ContainerOnNetwork);

    [Fact]
    public void TestGetNetworkUriFails() => this.AbstractTestGetNetworkUriFails(this.Container);

    protected override Uri GetNetworkUri(IMqttWebSocketsTlsContainer container, string? name = null) => container.GetNetworkWebSocketsTlsUri(name);

    protected override Uri GetUri(IMqttWebSocketsTlsContainer container, string? name = null) => container.GetWebSocketsTlsUri(name);
}
