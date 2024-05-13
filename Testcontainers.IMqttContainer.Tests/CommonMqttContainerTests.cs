// Ignore Spelling: MQTT

using DotNet.Testcontainers.Builders;
using Xunit.Abstractions;

namespace Testcontainers.Tests;

[Collection("Container")]
public abstract class CommonMqttContainerTests<TContainer, TBuilder>(ITestOutputHelper testOutputHelper, ContainerFixture<TContainer, TBuilder> containerFixture) : AbstractMqttContainerTests<TContainer, TBuilder, ICommonMqttContainer>(testOutputHelper, containerFixture)
    where TContainer : class, ICommonMqttContainer
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    [Fact]
    public Task TestCanConnectAsync() => AbstractTestCanConnectAsync(Container);

    [Fact]
    public void TestGetNetworkUriFails() => AbstractTestGetNetworkUriFails(Container);

    [Fact]
    public void TestGetNetworkUri() => AbstractTestGetNetworkUri(ContainerOnNetwork);

    protected override Uri GetUri(ICommonMqttContainer container, string? name = null) => container.GetMqttUri(name);

    protected override Uri GetNetworkUri(ICommonMqttContainer container, string? name = null) => container.GetNetworkMqttUri(name);
}
