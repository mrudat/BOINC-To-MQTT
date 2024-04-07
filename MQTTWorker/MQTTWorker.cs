using AsyncQueue;
using CommonStuff;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using static CommonStuff.IThingy;

namespace MQTTWorker;

public partial class MQTTWorker(
    ILogger<MQTTWorker> logger,
    IOptions<MQTTWorkerOptions> options,
    IGetHostCrossProjectIdentifier getHostCrossProjectIdentifier
    ) : BackgroundService, IThingy
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger _logger = logger;
    
    private readonly Dictionary<string, Subscription> subscriptions = [];

    private readonly AsyncQueue<MqttApplicationMessage> sendQueue = new();

    private readonly AsyncQueue<Subscription> subscribeQueue = new();

    private async Task<string> GetClientId(CancellationToken cancellationToken = default) => options.Value.ClientIdentifier ?? await getHostCrossProjectIdentifier.GetHostCrossProjectIdentifierAsync(cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
        var mqttFactory = new MqttFactory();

        using var mqttClient = mqttFactory.CreateMqttClient();

        var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(options.Value.HostName)
            .WithClientId(await GetClientId(stoppingToken));

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

        mqttClient.DisconnectedAsync += async e =>
        {
            //TODO exponential back off.
            if (e.ClientWasConnected)
            {
                await mqttClient.ConnectAsync(mqttClient.Options, stoppingToken);
            }
        };

        mqttClient.ApplicationMessageReceivedAsync += async eventArgs =>
        {
            var topic = eventArgs.ApplicationMessage.Topic;

            if (!subscriptions.TryGetValue(topic, out var subscription))
            {
                LogErrorUnhandledTopic(topic);
                return;
            }

            await subscription.HandleAsync(eventArgs);
        };

        var response = await mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);

        LogConnected();

        await Task.WhenAll([
            RegisterSubscriptions(mqttClient, stoppingToken),
            PublishMessages(mqttClient, stoppingToken)
        ]);

        var mqttClientDisconnectOptions = mqttFactory.CreateClientDisconnectOptionsBuilder().Build();

        await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);

        LogDisconnected();
    }

    public async Task RegisterSubscription(MqttClientSubscribeOptions subscribeOptions, MqttApplicationMessageReceivedEventHandler callback)
    {
        await subscribeQueue.EnqueueAsync(new Subscription(subscribeOptions, callback));
    }

    private async Task RegisterSubscriptions(IMqttClient mqttClient, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var subscription = await subscribeQueue.DequeueAsync(stoppingToken);

            foreach (var tf in subscription.subscribeOptions.TopicFilters)
                subscriptions[tf.Topic] = subscription;

            await mqttClient.SubscribeAsync(subscription.subscribeOptions, stoppingToken);
        }
    }

    public async Task PublishMessage(MqttApplicationMessage message, CancellationToken stoppingToken)
    {
        await sendQueue.EnqueueAsync(message, stoppingToken);
    }

    private async Task PublishMessages(IMqttClient mqttClient, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await mqttClient.PublishAsync(await sendQueue.DequeueAsync(stoppingToken), stoppingToken);
        }
    }

    private void ConfigureUsernameAndPassword(MqttClientOptionsBuilder mqttClientOptionsBuilder)
    {
        if (!options.Value.UserAndPasswordValid)
            return;
        mqttClientOptionsBuilder.WithCredentials(options.Value.User!, options.Value.Password!);
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Connected to MQTT Server")]
    private partial void LogConnected();

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Disconnected from MQTT Server")]
    private partial void LogDisconnected();

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "User name supplied without a password")]
    private partial void LogWarningUsernameWithoutPassword();

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Unhanded topic \"{topic}\"")]
    private partial void LogErrorUnhandledTopic(string topic);
}
