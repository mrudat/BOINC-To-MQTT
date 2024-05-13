// Ignore Spelling: MQTT

using DotNet.Testcontainers.Builders;
using FluentAssertions;
using MQTTnet.Adapter;
using Xunit.Abstractions;

namespace Testcontainers.Tests;

[Collection("Container")]
public abstract class AddUserContainerTests<TContainer, TBuilder>(ITestOutputHelper testOutputHelper, ContainerFixture<TContainer, TBuilder> containerFixture) : AbstractMqttContainerTests<TContainer, TBuilder, ICommonMqttContainer>(testOutputHelper, containerFixture)
    where TContainer : class, ICommonMqttContainer, IRequiresAuthentication
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    [Fact]
    public async Task TestCanNotConnectWithUnknownUserAsync()
    {
        Uri uri = GetUri(Container, "unknownUser");

        await this.Invoking(t => t.ConnectTo(uri)).Should().ThrowAsync<MqttConnectingFailedException>();
    }

    [Fact]
    public async Task TestCanConnectWithNewUserAsync()
    {
        IRequiresAuthentication addUser = Container as IRequiresAuthentication ?? throw new NullReferenceException("Shouldn't happen!");

        await addUser.AddUser("newUser", "newPassword");

        Uri uri = GetUri(Container, "newUser");

        using var mqttClient = await ConnectTo(uri);

        mqttClient.IsConnected.Should().BeTrue();
    }

    protected override Uri GetUri(ICommonMqttContainer container, string? name = null) => container.GetMqttUri(name);

    protected override Uri GetNetworkUri(ICommonMqttContainer container, string? name = null) => throw new NotImplementedException();
}
