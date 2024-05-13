// Ignore Spelling: BOINC MQTT
using DotNet.Testcontainers.Builders;
using Testcontainers;

namespace BOINC_To_MQTT.Tests;

public class MqttFixture<TContainer, TBuilder> : IMqttFixture, IAsyncLifetime
    where TContainer : class, IMqttContainer
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    public readonly TContainer Container = new TBuilder()
            // TODO capture container output.
            //.WithLogger(logger)
            .Build();

    public ICommonMqttContainer MqttContainer => Container;

    Task IAsyncLifetime.InitializeAsync()
    {
        return Container.StartAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => Container.DisposeAsync().AsTask();
}
