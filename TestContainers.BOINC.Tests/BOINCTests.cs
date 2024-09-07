// <copyright file="BOINCTests.cs" company="Martin Rudat">
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

namespace TestContainers.BOINC.Tests;

using BoincRpc;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public class BOINCTests(ITestOutputHelper testOutputHelper)
{
    private readonly ILogger<BOINCTests> logger = XUnitLogger.CreateLogger<BOINCTests>(testOutputHelper);

    [Fact]
    public async Task TestCanAuthenticate()
    {
        await using var container = new BoincBuilder()
            .WithLogger(this.logger)
            .Build();

        await container.StartAsync();

        var guiRpcKey = await container.GetGuiRpcKeyAsync();

        var rpcClient = new RpcClient();

        await rpcClient.ConnectAsync(container.Hostname, container.GuiRpcPort);

        rpcClient.Connected.Should().BeTrue();

        var authorized = await rpcClient.AuthorizeAsync(guiRpcKey);

        authorized.Should().BeTrue();
    }

    [Fact]
    public async Task TestCanConnect()
    {
        await using var container = new BoincBuilder()
            .WithLogger(this.logger)
            .Build();

        await container.StartAsync();

        var rpcClient = new RpcClient();

        await rpcClient.ConnectAsync(container.Hostname, container.GuiRpcPort);

        rpcClient.Connected.Should().BeTrue();
    }
}
