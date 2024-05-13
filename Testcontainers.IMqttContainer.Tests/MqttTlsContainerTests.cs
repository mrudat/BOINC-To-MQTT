// Ignore Spelling: MQTT TLS

using DotNet.Testcontainers.Builders;
using Xunit.Abstractions;

namespace Testcontainers.Tests;

[Collection("Container")]
public abstract class MqttTlsContainerTests<TContainer, TBuilder>(ITestOutputHelper testOutputHelper, ContainerFixture<TContainer, TBuilder> containerFixture) : AbstractMqttContainerTests<TContainer, TBuilder, IMqttTlsContainer>(testOutputHelper, containerFixture)
    where TContainer : class, IMqttTlsContainer
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    [Fact]
    public Task TestCanConnectAsync() => AbstractTestCanConnectAsync(Container);

    [Fact]
    public void TestGetNetworkUriFails() => AbstractTestGetNetworkUriFails(Container);

    [Fact]
    public void TestGetNetworkUri() => AbstractTestGetNetworkUri(ContainerOnNetwork);

    protected override Uri GetUri(IMqttTlsContainer container, string? name = null) => container.GetMqttTlsUri(name);

    protected override Uri GetNetworkUri(IMqttTlsContainer container, string? name = null) => container.GetNetworkMqttTlsUri(name);
}
