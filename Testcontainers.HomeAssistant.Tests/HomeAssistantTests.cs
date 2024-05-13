// Ignore Spelling: Mqtt

using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Testcontainers.Mosquitto;
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

    [Fact]
    public async Task TestCanPerformOnBoarding()
    {
        await using var container = new HomeAssistantBuilder()
            .WithLogger(logger)
            .Build();

        await container.StartAsync();

        await container.PerformOnBoardingAsync();
    }

    [Fact]
    public async Task TestCanPerformOnBoardingTwice()
    {
        await using var container = new HomeAssistantBuilder()
            .WithLogger(logger)
            .Build();

        await container.StartAsync();

        await container.PerformOnBoardingAsync();

        // TODO This does nothing, but how to prove it?
        await container.PerformOnBoardingAsync();
    }

    [Fact]
    public async Task TestCanConnectToMqtt()
    {
        await using var network = new NetworkBuilder()
            .WithLogger(logger)
            .Build();

        await using var container = new HomeAssistantBuilder()
            .WithLogger(logger)
            .WithNetwork(network)
            .WithNetworkAliases("home-assistant")
            .Build();

        await using var mqttContainer = new MosquittoBuilder()
            .WithLogger(logger)
            .WithNetwork(network)
            .WithNetworkAliases("mosquitto")
            .Build();

        await container.StartAsync();

        await container.PerformOnBoardingAsync();

        await mqttContainer.StartAsync();

        await container.ConfigureMqttAsync(mqttContainer);
    }

    [Fact]
    public async Task TestCanConnectToMqttTwice()
    {
        await using var network = new NetworkBuilder()
            .WithLogger(logger)
            .Build();

        await using var container = new HomeAssistantBuilder()
            .WithLogger(logger)
            .WithNetwork(network)
            .WithNetworkAliases("home-assistant")
            .Build();

        await using var mqttContainer = new MosquittoBuilder()
            .WithLogger(logger)
            .WithNetwork(network)
            .WithNetworkAliases("mosquitto")
            .Build();

        await container.StartAsync();

        await container.PerformOnBoardingAsync();

        await mqttContainer.StartAsync();

        await container.ConfigureMqttAsync(mqttContainer);

        // TODO This (currently) does nothing, but how to prove it?
        // TODO if we specify a different container, it should update the configuration.
        await container.ConfigureMqttAsync(mqttContainer);
    }

    [Fact]
    public async Task TestCanConnectToMqttWithoutExplicitOnBoarding()
    {
        await using var network = new NetworkBuilder()
            .WithLogger(logger)
            .Build();

        await using var container = new HomeAssistantBuilder()
            .WithLogger(logger)
            .WithNetwork(network)
            .WithNetworkAliases("home-assistant")
            .Build();

        await using var mqttContainer = new MosquittoBuilder()
            .WithLogger(logger)
            .WithNetwork(network)
            .WithNetworkAliases("mosquitto")
            .Build();

        await container.StartAsync();

        await mqttContainer.StartAsync();

        await container.ConfigureMqttAsync(mqttContainer);
    }
}