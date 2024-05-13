// Ignore Spelling: BOINC MQTT homeassistant frontend username offline
using DotNet.Testcontainers.Builders;
using Testcontainers;
using Testcontainers.HomeAssistant;

namespace BOINC_To_MQTT.Tests;

[Collection(nameof(MqttCollection))]
public class HomeAssistantFixture<TMqttContainer, TMqttBuilder>(MqttFixture<TMqttContainer, TMqttBuilder> mqttFixture) : IAsyncLifetime, IHomeAssistantFixture
    where TMqttContainer : class, IMqttContainer
    where TMqttBuilder : class, IContainerBuilder<TMqttBuilder, TMqttContainer>, new()
{
    private HomeAssistantContainer? container;

    public HomeAssistantContainer Container => container!;

    IMqttContainer IHomeAssistantFixture.MqttContainer => mqttFixture.Container;

    async Task IAsyncLifetime.InitializeAsync()
    {
        CancellationTokenSource cancellationTokenSource = new();

        var cancellationToken = cancellationTokenSource.Token;

        container = new HomeAssistantBuilder()
            //.WithLogger(logger)
            .Build();

        await container.StartAsync(cancellationToken);

        await container.PerformOnBoardingAsync(cancellationToken);

        if (mqttFixture.Container is IRequiresAuthentication addUser)
            await addUser.AddUser("homeassistant", "homeassistant", cancellationToken);

        await container.ConfigureMqttAsync(mqttFixture.Container.GetMqttUri("homeassistant"), cancellationToken);
    }

    Task IAsyncLifetime.DisposeAsync() => container?.DisposeAsync().AsTask() ?? Task.CompletedTask;
}