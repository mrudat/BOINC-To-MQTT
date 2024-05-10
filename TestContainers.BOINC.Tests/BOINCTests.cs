using BoincRpc;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TestContainers.BOINC.Tests;

public class BOINCTests(ITestOutputHelper testOutputHelper)
{
    private readonly ILogger<BOINCTests> logger = XUnitLogger.CreateLogger<BOINCTests>(testOutputHelper);

    [Fact]
    public async Task TestCanConnect()
    {
        await using var container = new BoincBuilder()
            .WithLogger(logger)
            .Build();

        await container.StartAsync();

        var rpcClient = new RpcClient();

        await rpcClient.ConnectAsync(container.Hostname, container.Port);

        rpcClient.Connected.Should().BeTrue();
    }

    [Fact]
    public async Task TestCanAuthenticate()
    {
        await using var container = new BoincBuilder()
            .WithLogger(logger)
            .Build();

        await container.StartAsync();

        var guiRpcKey = await container.GetGuiRpcKeyAsync();

        var rpcClient = new RpcClient();

        await rpcClient.ConnectAsync(container.Hostname, container.Port);

        rpcClient.Connected.Should().BeTrue();

        var authorized = await rpcClient.AuthorizeAsync(guiRpcKey);

        authorized.Should().BeTrue();
    }
}