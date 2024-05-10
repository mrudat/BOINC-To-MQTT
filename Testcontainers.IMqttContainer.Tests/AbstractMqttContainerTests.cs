// Ignore Spelling: MQTT Testcontainers Initialize TLS initialization URI mqtts wss

using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client;
using Xunit.Abstractions;

namespace Testcontainers.Tests;

public abstract partial class AbstractMqttContainerTests<TContainer, TBuilder> : IAsyncLifetime
    where TContainer : class, ICommonMqttContainer
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    protected readonly ITestOutputHelper testOutputHelper;

    protected readonly ILogger<AbstractMqttContainerTests<TContainer, TBuilder>> logger;

    protected readonly MqttFactory mqttFactory = new();

    protected readonly TContainer Container;

    public static TheoryData<bool> IsICommonMqttContainer { get; } = Implements<ICommonMqttContainer>();
    public static TheoryData<bool> IsIMqttContainer { get; } = Implements<IMqttContainer>();
    public static TheoryData<bool> IsIMqttTlsContainer { get; } = Implements<IMqttTlsContainer>();
    public static TheoryData<bool> IsIMqttWebSocketsContainer { get; } = Implements<IMqttWebSocketsContainer>();
    public static TheoryData<bool> IsIMqttWebSocketsTlsContainer { get; } = Implements<IMqttWebSocketsTlsContainer>();

    public static TheoryData<bool> IsIAddUser { get; } = Implements<IAddUser>();

    protected AbstractMqttContainerTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;

        logger = XUnitLogger.CreateLogger<AbstractMqttContainerTests<TContainer, TBuilder>>(testOutputHelper);

        Container = new TBuilder().WithLogger(logger).Build();
    }

    Task IAsyncLifetime.InitializeAsync() => Container.StartAsync();

    Task IAsyncLifetime.DisposeAsync() => Container.DisposeAsync().AsTask();

    public static bool Implements2<TInterface>() => typeof(TContainer).IsAssignableTo(typeof(TInterface));

    public static TheoryData<bool> Implements<TInterface>()
    {
        var theoryData = new TheoryData<bool>();
        if (typeof(TContainer).IsAssignableTo(typeof(TInterface)))
            theoryData.Add(true);
        return theoryData;
    }

    [Theory]
    [MemberData(nameof(IsICommonMqttContainer))]
    public Task TestCanConnectCommonAsync(bool _) => AbstractTestCanConnectAsync<ICommonMqttContainer>(Container);

    [Theory]
    [MemberData(nameof(IsIMqttContainer))]
    public Task TestCanConnectMqttAsync(bool _) => AbstractTestCanConnectAsync((IMqttContainer)Container);

    [Theory]
    [MemberData(nameof(IsIMqttTlsContainer))]
    public Task TestCanConnectMqttTlsAsync(bool _) => AbstractTestCanConnectAsync((IMqttTlsContainer)Container);

    [Theory]
    [MemberData(nameof(IsIMqttWebSocketsContainer))]
    public Task TestCanConnectMqttWebSocketsAsync(bool _) => AbstractTestCanConnectAsync((IMqttWebSocketsContainer)Container);

    [Theory]
    [MemberData(nameof(IsIMqttWebSocketsTlsContainer))]
    public Task TestCanConnectMqttWebSocketsTlsAsync(bool _) => AbstractTestCanConnectAsync((IMqttWebSocketsTlsContainer)Container);

    private async Task AbstractTestCanConnectAsync<TInterface>(TInterface container)
    where TInterface : ICommonMqttContainer
    {
        Uri uri = GetUri(container);

        using var mqttClient = await ConnectTo(uri);

        mqttClient.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task TestCanNotConnectWithUnknownUserAsync()
    {
        var action = async () =>
        {
            Uri unkownUserUri = GetUri(Container, "unknownUser");

            using var unknownUserMqttClient = await ConnectTo(unkownUserUri);

            // unreachable?
            unknownUserMqttClient.IsConnected.Should().BeFalse();
        };
        await action.Should().ThrowAsync<MqttConnectingFailedException>();
    }

    [Theory]
    [MemberData(nameof(IsIAddUser))]
    public async Task TestCanConnectWithNewUserAsync(bool _)
    {
        IAddUser addUser = Container as IAddUser ?? throw new NullReferenceException("Shouldn't happen!");

        await addUser.AddUser("newUser", "newPassword");

        Uri newUserUri = GetUri(Container, "newUser");

        using var newUserMqttClient = await ConnectTo(newUserUri);

        newUserMqttClient.IsConnected.Should().BeTrue();
    }

    protected Uri GetUri(ICommonMqttContainer container, string? name = null) => container.GetMqttUri(name);
    protected Uri GetUri(IMqttContainer container, string? name = null) => container.GetMqttUri(name);
    protected Uri GetUri(IMqttTlsContainer container, string? name = null) => container.GetMqttTlsUri(name);
    protected Uri GetUri(IMqttWebSocketsContainer container, string? name = null) => container.GetWebSocketsUri(name);
    protected Uri GetUri(IMqttWebSocketsTlsContainer container, string? name = null) => container.GetWebSocketsTlsUri(name);

    private async Task<IMqttClient> ConnectTo(Uri uri)
    {
        var mqttClient = mqttFactory.CreateMqttClient();

        var mqttClientOptionsBuilder = mqttFactory.CreateClientOptionsBuilder();

#pragma warning disable CS0618 // Type or member is obsolete - No valid replacement for WithConnectionUri
        mqttClientOptionsBuilder
            .WithConnectionUri(uri);
#pragma warning restore CS0618 // Type or member is obsolete

        var usingTls = uri.Scheme.ToLowerInvariant() switch
        {
            "mqtts" or "wss" => true,
            _ => false,
        };

        if (usingTls)
        {
            // TODO fetch certificate to compare
            //var cert = await (container as IMqttTlsContainer)?.GetServerCertificateAsync();

            mqttClientOptionsBuilder
                .WithTlsOptions((opt) =>
                {
                    opt.WithCertificateValidationHandler((args) =>
                    {
                        //return args.Certificate.Equals(cert);
                        return true;
                    });
                });
        }

        var mqttClientOptions = mqttClientOptionsBuilder
            .Build();

        LogConnecting(uri);

        await mqttClient.ConnectAsync(mqttClientOptions);

        return mqttClient;
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Connecting to {uri}")]
    private partial void LogConnecting(Uri uri);

    public class FactIfImplementsInterfaceAttribute<TInterface> : FactAttribute
    {
        public FactIfImplementsInterfaceAttribute()
        {
            var objectType = typeof(TContainer);
            var interfaceType = typeof(TInterface);

            if (!objectType.IsAssignableTo(interfaceType))
            {
                Skip = $"{objectType.Name} does not implement {interfaceType.Name}";
            }

        }
    }

}
