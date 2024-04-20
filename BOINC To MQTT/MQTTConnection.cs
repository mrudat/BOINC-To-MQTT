using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace BOINC_To_MQTT;

internal partial class MQTTConnection(
    ILogger<MQTTConnection> logger,
    IOptions<BOINC2MQTTWorkerOptions> options,
    IBOINCConnection bOINCClient
    ) : IHostedService, IMQTTConnection
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger _logger = logger;

    private readonly MqttFactory mqttFactory = new();

    private IMqttClient? client = null;

    private readonly Dictionary<string, Subscription> subscriptions = [];

    public async Task RegisterSubscription(string topic, SubscriptionCallback callback, CancellationToken cancellationToken = default)
    {
        var mqttClient = await GetMQTTClient(cancellationToken);

        MqttClientSubscribeOptionsBuilder foo = new();
        foo.WithTopicFilter(topic);
        var subscription = new Subscription(foo.Build(), callback);

        foreach (var tf in subscription.subscribeOptions.TopicFilters)
            subscriptions[tf.Topic] = subscription;

        await mqttClient.SubscribeAsync(subscription.subscribeOptions, cancellationToken);
    }

    private async Task<IMqttClient> GetMQTTClient(CancellationToken cancellationToken = default)
    {
        if (client == null)
        {
            var clientId = await bOINCClient.GetClientIdentifierAsync(cancellationToken);

            client = mqttFactory.CreateMqttClient();

            var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(options.Value.MQTT.HostName)
                .WithClientId(clientId);

            ConfigureUsernameAndPassword(mqttClientOptionsBuilder);

            // TODO require encryption if using user name/password

            mqttClientOptionsBuilder.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500);

            var tlsOptionsBuilder = new MqttClientTlsOptionsBuilder();

            tlsOptionsBuilder.UseTls();
            tlsOptionsBuilder.WithAllowRenegotiation(true);
            tlsOptionsBuilder.WithAllowUntrustedCertificates(true);

            // TODO first try SSL, and fall-back to unencrypted if that fails.

            mqttClientOptionsBuilder.WithTlsOptions(tlsOptionsBuilder.Build());

            var mqttClientOptions = mqttClientOptionsBuilder
                .Build();

            client.DisconnectedAsync += async e =>
            {
                //TODO exponential back-off.
                if (e.ClientWasConnected)
                {
                    await client.ConnectAsync(client.Options, cancellationToken);
                }
            };

            client.ApplicationMessageReceivedAsync += async eventArgs =>
            {
                var topic = eventArgs.ApplicationMessage.Topic;

                if (!subscriptions.TryGetValue(topic, out var subscription))
                {
                    LogErrorUnhandledTopic(topic);
                    return;
                }

                await subscription.callback(topic, eventArgs.ApplicationMessage.ConvertPayloadToString());
            };

            // TODO exponential back-off
            var response = await client.ConnectAsync(mqttClientOptions, cancellationToken);

            LogConnected();
        }
        return client;
    }

    private void ConfigureUsernameAndPassword(MqttClientOptionsBuilder mqttClientOptionsBuilder)
    {
        if (!options.Value.MQTT.UserAndPasswordValid)
            return;
        mqttClientOptionsBuilder.WithCredentials(options.Value.MQTT.User!, options.Value.MQTT.Password!);
    }


    public async Task PublishMessage(string topic, string payload, bool retain = false, CancellationToken cancellationToken = default)
    {
        var client = await GetMQTTClient(cancellationToken);

        MqttApplicationMessageBuilder mb = new();
        mb.WithPayload(payload);
        mb.WithTopic(topic);
        if (retain)
            mb.WithRetainFlag(true);

        await client.PublishAsync(mb.Build(), cancellationToken);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await GetMQTTClient(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var clientId = await bOINCClient.GetClientIdentifierAsync(cancellationToken);

        await PublishMessage(
            $"boinc2mqtt/{clientId}/available",
            "offline",
            retain: true,
            cancellationToken: cancellationToken
        );

        var mqttClientDisconnectOptions = mqttFactory.CreateClientDisconnectOptionsBuilder()
        .Build();

        if (client != null)
            await client.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);

        LogDisconnected();
    }


    [LoggerMessage(EventId = (int)EventIdentifier.ConnectedToMQTTServer, Level = LogLevel.Information, Message = "Connected to MQTT Server")]
    private partial void LogConnected();

    [LoggerMessage(EventId = (int)EventIdentifier.DisconnectedFromMQTTServer, Level = LogLevel.Information, Message = "Disconnected from MQTT Server")]
    private partial void LogDisconnected();

    [LoggerMessage(EventId = (int)EventIdentifier.IncorrectAuthentication, Level = LogLevel.Warning, Message = "User name supplied without a password")]
    private partial void LogWarningUsernameWithoutPassword();

    [LoggerMessage(EventId = (int)EventIdentifier.UnhandledTopic, Level = LogLevel.Error, Message = "Unhanded topic \"{topic}\"")]
    private partial void LogErrorUnhandledTopic(string topic);
}

public delegate Task SubscriptionCallback(string topic, string payload, CancellationToken cancellationToken = default);
