// Ignore Spelling: MQTT

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

namespace Testcontainers.Tests;

public abstract class ContainerFixture<TContainer, TBuilder> : IAsyncLifetime
    where TContainer : class, ICommonMqttContainer
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    public TContainer Container { get; }

    public INetwork Network { get; }

    public TContainer ContainerOnNetwork { get; }

    protected ContainerFixture()
    {
        // TODO logging?
        Container = new TBuilder()
            .Build();

        Network = new NetworkBuilder()
            .Build();

        ContainerOnNetwork = new TBuilder()
            .WithNetwork(Network)
            .WithNetworkAliases(nameof(TContainer))
            .Build();
    }

    Task IAsyncLifetime.InitializeAsync()
    {
        return Task.WhenAll([
            Container.StartAsync(),
            ContainerOnNetwork.StartAsync()
            ]);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await Task.WhenAll([
            Container.DisposeAsync().AsTask(),
            ContainerOnNetwork.DisposeAsync().AsTask(),
            ]);
        await Network.DisposeAsync();
    }
}
