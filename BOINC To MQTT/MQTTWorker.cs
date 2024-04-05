using AsyncQueue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BOINC_To_MQTT
{
    internal partial class MQTTWorker(
        Lazy<ClientId> clientId, // TODO AsyncLazy
        ILogger<MQTTWorker> logger,
        IConfiguration configuration) : BackgroundService
    {
        // workaround for https://github.com/dotnet/runtime/issues/91121
        private readonly ILogger _logger = logger;

        private readonly Dictionary<string, Subscription> subscriptions = [];

        private readonly AsyncQueue<MqttApplicationMessage> sendQueue = new();

        private readonly AsyncQueue<Subscription> subscribeQueue = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mqttFactory = new MqttFactory();

            using var mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(configuration.GetValue("hostname", "localhost"))
                .WithClientId(clientId.Value.Id);

            ConfigureUsernameAndPassword(mqttClientOptionsBuilder);

            // TODO require encryption if using username/password

            mqttClientOptionsBuilder.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500);

            var tlsOptionsBuilder = new MqttClientTlsOptionsBuilder();

            tlsOptionsBuilder.UseTls();
            tlsOptionsBuilder.WithAllowRenegotiation(true);
            tlsOptionsBuilder.WithAllowUntrustedCertificates(true);

            // TODO first try SSL, and fallback to unencrypted if that fails.

            mqttClientOptionsBuilder.WithTlsOptions(tlsOptionsBuilder.Build());

            var mqttClientOptions = mqttClientOptionsBuilder
                .Build();

            mqttClient.DisconnectedAsync += async e =>
            {
                //TODO exponential backoff.
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

        internal async Task RegisterSubscription(MqttClientSubscribeOptions subscribeOptions, Func<MqttApplicationMessageReceivedEventArgs, Task> callback)
        {
            await subscribeQueue.EnqueueAsync(new Subscription(subscribeOptions, callback));
        }

        private async Task RegisterSubscriptions(IMqttClient mqttClient, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var subscription = await subscribeQueue.DequeueAsync(stoppingToken);

                foreach (var tf in subscription.subscribeOptions.TopicFilters)
                {
                    subscriptions[tf.Topic] = subscription;
                }

                await mqttClient.SubscribeAsync(subscription.subscribeOptions, stoppingToken);
            }
        }

        internal async Task PublishMessage(MqttApplicationMessage message, CancellationToken stoppingToken)
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
            var username = configuration.GetValue<string>("username");

            if (username != null)
            {
                var password = configuration.GetValue<string>("password");

                if (password != null)
                {
                    mqttClientOptionsBuilder.WithCredentials(username, password);
                }
                else
                {
                    LogWarningUsernameWithoutPassword();
                }
            }
        }

        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Connected to MQTT Server")]
        private partial void LogConnected();

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Disconnected from MQTT Server")]
        private partial void LogDisconnected();

        [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Username supplied without a password")]
        private partial void LogWarningUsernameWithoutPassword();

        [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Unhandled topic \"{topic}\"")]
        private partial void LogErrorUnhandledTopic(string topic);

        internal class Subscription(MqttClientSubscribeOptions subscribeOptions, Func<MqttApplicationMessageReceivedEventArgs, Task> handleAsync)
        {
            internal readonly MqttClientSubscribeOptions subscribeOptions = subscribeOptions;

            internal readonly Func<MqttApplicationMessageReceivedEventArgs, Task> HandleAsync = handleAsync;
        }
    }
}
