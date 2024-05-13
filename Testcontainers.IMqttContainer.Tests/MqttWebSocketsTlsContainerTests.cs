// Ignore Spelling: MQTT TLS

using DotNet.Testcontainers.Builders;
using Xunit.Abstractions;

namespace Testcontainers.Tests;

[Collection("Container")]
public abstract class MqttWebSocketsTlsContainerTests<TContainer, TBuilder>(ITestOutputHelper testOutputHelper, ContainerFixture<TContainer, TBuilder> containerFixture) : AbstractMqttContainerTests<TContainer, TBuilder, IMqttWebSocketsTlsContainer>(testOutputHelper, containerFixture)
    where TContainer : class, IMqttWebSocketsTlsContainer
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    [Fact]
    public Task TestCanConnectAsync() => AbstractTestCanConnectAsync(Container);

    [Fact]
    public void TestGetNetworkUriFails() => AbstractTestGetNetworkUriFails(Container);

    [Fact]
    public void TestGetNetworkUri() => AbstractTestGetNetworkUri(ContainerOnNetwork);

    protected override Uri GetUri(IMqttWebSocketsTlsContainer container, string? name = null) => container.GetWebSocketsTlsUri(name);

    protected override Uri GetNetworkUri(IMqttWebSocketsTlsContainer container, string? name = null) => container.GetNetworkWebSocketsTlsUri(name);
}