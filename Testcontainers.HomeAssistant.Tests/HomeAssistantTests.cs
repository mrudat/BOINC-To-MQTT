using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Testcontainers.HomeAssistant.Tests;

public class HomeAssistantTests(ITestOutputHelper testOutputHelper)
{
    private readonly ILogger<HomeAssistantTests> logger = XUnitLogger.CreateLogger<HomeAssistantTests>(testOutputHelper);

    [Fact]
    public async Task TestCanStart()
    {
        await using var container = new HomeAssistantBuilder()
            .WithLogger(logger)
            .Build();

        await container.StartAsync();

        var result = await container.CheckConfig();

        testOutputHelper.WriteLine(result.Stderr);
        testOutputHelper.WriteLine(result.Stdout);

        result.ExitCode.Should().Be(0);
    }
}