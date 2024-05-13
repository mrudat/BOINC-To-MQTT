// Ignore Spelling: MQTT

using DotNet.Testcontainers.Builders;
using Xunit.Abstractions;

namespace Testcontainers.Tests;

[Collection("Container")]
public abstract class MqttWebSocketsContainerTests<TContainer, TBuilder>(ITestOutputHelper testOutputHelper, ContainerFixture<TContainer, TBuilder> containerFixture) : AbstractMqttContainerTests<TContainer, TBuilder, IMqttWebSocketsContainer>(testOutputHelper, containerFixture)
    where TContainer : class, IMqttWebSocketsContainer
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    [Fact]
    public Task TestCanConnectAsync() => AbstractTestCanConnectAsync(Container);

    [Fact]
    public void TestGetNetworkUriFails() => AbstractTestGetNetworkUriFails(Container);

    [Fact]
    public void TestGetNetworkUri() => AbstractTestGetNetworkUri(ContainerOnNetwork);

    protected override Uri GetUri(IMqttWebSocketsContainer container, string? name = null) => container.GetWebSocketsUri(name);

    protected override Uri GetNetworkUri(IMqttWebSocketsContainer container, string? name = null) => container.GetNetworkWebSocketsUri(name);
}
