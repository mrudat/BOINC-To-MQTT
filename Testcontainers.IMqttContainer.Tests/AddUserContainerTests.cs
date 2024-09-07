// <copyright file="AddUserContainerTests.cs" company="Martin Rudat">
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
using FluentAssertions;
using MQTTnet.Adapter;
using Xunit.Abstractions;

[Collection("Container")]
public abstract class AddUserContainerTests<TContainer, TBuilder>(ITestOutputHelper testOutputHelper, ContainerFixture<TContainer, TBuilder> containerFixture) : AbstractMqttContainerTests<TContainer, TBuilder, ICommonMqttContainer>(testOutputHelper, containerFixture)
    where TContainer : class, ICommonMqttContainer, IRequiresAuthentication
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    [Fact]
    public async Task TestCanConnectWithNewUserAsync()
    {
        IRequiresAuthentication addUser = this.Container as IRequiresAuthentication ?? throw new NullReferenceException("Shouldn't happen!");

        await addUser.AddUser("newUser", "newPassword");

        Uri uri = this.GetUri(this.Container, "newUser");

        using var mqttClient = await this.ConnectTo(uri);

        mqttClient.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task TestCanNotConnectWithUnknownUserAsync()
    {
        Uri uri = this.GetUri(this.Container, "unknownUser");

        await this.Invoking(t => t.ConnectTo(uri)).Should().ThrowAsync<MqttConnectingFailedException>();
    }

    protected override Uri GetNetworkUri(ICommonMqttContainer container, string? name = null) => throw new NotImplementedException();

    protected override Uri GetUri(ICommonMqttContainer container, string? name = null) => container.GetMqttUri(name);
}
