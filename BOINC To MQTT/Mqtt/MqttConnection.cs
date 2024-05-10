// Ignore Spelling: offline MQTT unregister

using BOINC_To_MQTT.Scaffolding;
using DotNext.Threading;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System.IO.Abstractions;
using System.Security.Cryptography;

namespace BOINC_To_MQTT.Mqtt;

internal partial class MqttConnection : IMqttConnection, IAsyncDisposable, IAsyncInitialise<MqttConnection>
{
    private readonly ILogger logger;
    private readonly Boinc2MqttOptions options;
    private readonly IHostEnvironment hostEnvironment;
    private readonly IFileSystem fileSystem;

    private readonly MqttFactory mqttFactory = new();

    internal readonly IManagedMqttClient mqttClient;

    private string? clientIdentifier;

    private readonly Dictionary<string, MqttSubscription> subscriptions = [];

    private readonly AsyncReaderWriterLock subscriptionsLock = new();

    internal static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddSingleton<MqttConnection>()
            .AddSingleton<IMqttConnection, MqttConnectionWrapper>();
    }

    internal class MqttConnectionWrapper(MqttConnection mqttConnection2) : IMqttConnection
    {
        // FIXME exception isn't propagated!
        private readonly AsyncLazy<MqttConnection> mqttConnection = new((ct) => (mqttConnection2 as IAsyncInitialise<MqttConnection>).Initialise(ct));

        async Task IMqttConnection.PublishMessage(string topic, string payload, bool retain, CancellationToken cancellationToken) => await (await mqttConnection.WithCancellation(cancellationToken) as IMqttConnection).PublishMessage(topic, payload, retain, cancellationToken);

        async Task<MqttSubscription> IMqttConnection.RegisterSubscription(string topic, SubscriptionCallback callback, CancellationToken cancellationToken) => await (await mqttConnection.WithCancellation(cancellationToken) as IMqttConnection).RegisterSubscription(topic, callback, cancellationToken);
    }

    public MqttConnection(
        ILogger<MqttConnection> logger,
        IOptions<Boinc2MqttOptions> options,
        IHostEnvironment hostEnvironment,
        IFileSystem fileSystem
    )
    {
        this.logger = logger;
        this.options = options.Value;
        this.hostEnvironment = hostEnvironment;
        this.fileSystem = fileSystem;

        mqttClient = mqttFactory.CreateManagedMqttClient();
    }

    async Task<MqttConnection> IAsyncInitialise<MqttConnection>.Initialise(CancellationToken cancellationToken)
    {
        var clientId = await GetClientIdentifierAsync(cancellationToken);

#pragma warning disable CS0618 // Type or member is obsolete - No valid replacement for WithConnectionUri
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithConnectionUri(options.MQTT.Uri)
            .WithClientId(clientId)
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
            .WithWillTopic($"boinc2mqtt/{clientId}/available")
            .WithWillPayload("offline")
            .WithWillRetain(true)
            .Build();
#pragma warning restore CS0618 // Type or member is obsolete

        var managedMqttClientOptions = mqttFactory.CreateManagedMqttClientOptionsBuilder()
            .WithClientOptions(mqttClientOptions)
            .Build();

        mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceivedAsync;

        AsyncTaskCompletionSource tcs = new();

        Task ConnectedAsync(MqttClientConnectedEventArgs args) => args.ConnectResult.ResultCode == MqttClientConnectResultCode.Success ? tcs.TrySetResult() : tcs.TrySetException(new Exception(args.ConnectResult.ReasonString));

        Task ConnectingFailedAsync(ConnectingFailedEventArgs args)
        {
            if (args.ConnectResult != null)
            {
                switch (args.ConnectResult.ResultCode)
                {
                    case MqttClientConnectResultCode.ServerBusy:
                        // TODO delay.
                        return Task.CompletedTask;
                }
            }
            return tcs.TrySetException(args.Exception);
        }

        mqttClient.ConnectedAsync += ConnectedAsync;

        mqttClient.ConnectingFailedAsync += ConnectingFailedAsync;

        await mqttClient.StartAsync(managedMqttClientOptions);

        await tcs.Task.WaitAsync(cancellationToken);

        mqttClient.ConnectedAsync -= ConnectedAsync;

        mqttClient.ConnectingFailedAsync -= ConnectingFailedAsync;

        return this;
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        var clientId = await GetClientIdentifierAsync();

        mqttClient.ApplicationMessageReceivedAsync -= ApplicationMessageReceivedAsync;

        List<Task> taskList;

        using (var _ = await subscriptionsLock.AcquireWriteLockAsync(CancellationToken.None))
        {
            taskList = new(subscriptions.Values.Select(s => s.DisposeAsync().AsTask()));
        }

        if (mqttClient.IsConnected)
        {
            await ((IMqttConnection)this).PublishMessage(
                $"boinc2mqtt/{clientId}/available",
                "offline",
                retain: true
            );

            while (mqttClient.PendingApplicationMessagesCount > 0)
            {
                await Task.Yield();
            }
        }

        await mqttClient.StopAsync(true);
        await Task.WhenAll(taskList);

        LogDisconnected();
    }

    async Task IMqttConnection.PublishMessage(string topic, string payload, bool retain, CancellationToken cancellationToken)
    {
        var mb = mqttFactory.CreateApplicationMessageBuilder()
            .WithPayload(payload)
            .WithTopic(topic);

        if (retain)
            mb.WithRetainFlag(true);

        await mqttClient.EnqueueAsync(mb.Build());
    }

    async Task<MqttSubscription> IMqttConnection.RegisterSubscription(string topic, SubscriptionCallback callback, CancellationToken cancellationToken)
    {
        var subscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(topic)
            .Build();

        var subscription = new MqttSubscription(
            topic,
            callback,
            UnregisterSubscriptionAsync
        );

        var alreadySubscribed = false;

        using (var _ = await subscriptionsLock.AcquireWriteLockAsync(cancellationToken))
        {
            alreadySubscribed = subscriptions.ContainsKey(topic);

            subscriptions[topic] = subscription;
        };

        if (!alreadySubscribed)
            await mqttClient.SubscribeAsync(topic);

        return subscription;
    }

    internal async Task ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;

        if (!subscriptions.TryGetValue(topic, out var subscription))
        {
            LogWarningUnhandledTopic(topic);
            return;
        }

        await subscription.SubscriptionCallback(topic, args.ApplicationMessage.ConvertPayloadToString());
    }

    private async Task UnregisterSubscriptionAsync(MqttSubscription subscription, CancellationToken cancellationToken = default)
    {
        using (var _ = await subscriptionsLock.AcquireWriteLockAsync(cancellationToken))
        {
            if (!subscriptions.TryGetValue(subscription.Topic, out var currentSubscription) || currentSubscription != subscription)
            {
                return;
            }

            subscriptions.Remove(subscription.Topic);
        }

        await mqttClient.UnsubscribeAsync(subscription.Topic);
    }

    // from https://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html#_Toc385349242
    public const string ClientIdentifierCharacters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private async Task<string> GetClientIdentifierAsync(CancellationToken cancellationToken = default)
    {
        clientIdentifier ??= await GetClientIdentifierAsync2(cancellationToken);
        return clientIdentifier;
    }

    private async Task<string> GetClientIdentifierAsync2(CancellationToken cancellationToken = default)
    {
        if (options.MQTT.ClientIdentifier != null)
            return options.MQTT.ClientIdentifier;

        var applicationPath = hostEnvironment.ContentRootPath;

        var clientIdentifierPath = fileSystem.Path.Join(applicationPath, "ClientIdentifier.txt");

        try
        {
            return await fileSystem.File.ReadLinesAsync(clientIdentifierPath, cancellationToken).FirstAsync(cancellationToken);
        }
        catch (FileNotFoundException)
        {
            var clientIdentifier = GenerateNewClientIdentifier();

            await fileSystem.File.WriteAllTextAsync(clientIdentifierPath, clientIdentifier, cancellationToken);

            return clientIdentifier;
        }
    }

    private static string GenerateNewClientIdentifier()
    {
        return new string(RandomNumberGenerator.GetItems<char>(ClientIdentifierCharacters, 23));
    }

    [LoggerMessage(EventId = (int)EventIdentifier.ConnectedToMQTTServer, Level = LogLevel.Information, Message = "Connected to MQTT Server")]
    private partial void LogConnected();

    [LoggerMessage(EventId = (int)EventIdentifier.DisconnectedFromMQTTServer, Level = LogLevel.Information, Message = "Disconnected from MQTT Server")]
    private partial void LogDisconnected();

    [LoggerMessage(EventId = (int)EventIdentifier.UnhandledTopic, Level = LogLevel.Warning, Message = "Unhanded topic \"{topic}\"")]
    private partial void LogWarningUnhandledTopic(string topic);
}


