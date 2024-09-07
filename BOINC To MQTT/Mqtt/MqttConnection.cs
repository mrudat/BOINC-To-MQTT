// <copyright file="MqttConnection.cs" company="Martin Rudat">
// BOINC To MQTT - Exposes some BOINC controls via MQTT for integration with Home Assistant.
// Copyright (C) 2024  Martin Rudat
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>

namespace BOINC_To_MQTT.Mqtt;

using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;
using BOINC_To_MQTT.Scaffolding;
using DotNext.Threading;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

/// <inheritdoc cref="IMqttConnection"/>
internal partial class MqttConnection : IMqttConnection, IAsyncDisposable, IAsyncInitialise<MqttConnection>, IHostApplicationBuilderConfiguration
{
    /// <summary>
    /// The set of valid characters to user in a client identifier from https://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html#_Toc385349242.
    /// </summary>
    public const string ClientIdentifierCharacters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private readonly AsyncLazy<string> clientIdentifer;
    private readonly IFileSystem fileSystem;
    private readonly IHostEnvironment hostEnvironment;
    private readonly ILogger logger;
    private readonly IManagedMqttClient mqttClient;
    private readonly MqttFactory mqttFactory = new();
    private readonly Boinc2MqttOptions options;
    private readonly Dictionary<string, MqttSubscription> subscriptions = [];

    private readonly AsyncReaderWriterLock subscriptionsLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttConnection"/> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="fileSystem"></param>
    public MqttConnection(
        ILogger<MqttConnection> logger,
        IOptions<Boinc2MqttOptions> options,
        IHostEnvironment hostEnvironment,
        IFileSystem? fileSystem)
    {
        this.logger = logger;
        this.options = options.Value;
        this.hostEnvironment = hostEnvironment;
        this.fileSystem = fileSystem ?? new FileSystem();

        this.mqttClient = this.mqttFactory.CreateManagedMqttClient();

        this.clientIdentifer = new(this.GetOrMakeClientIdentifierAsync);
    }

    /// <inheritdoc/>
    public static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddSingleton<MqttConnection>()
            .AddSingleton<IMqttConnection, MqttConnectionWrapper>();
    }

    /// <inheritdoc/>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        var clientId = await ((IMqttConnection)this).GetClientIdentifierAsync().ConfigureAwait(false);

        this.mqttClient.ApplicationMessageReceivedAsync -= this.ApplicationMessageReceivedAsync;

        List<Task> taskList;

        using (var asyncLockHolder = await this.subscriptionsLock.AcquireWriteLockAsync(CancellationToken.None).ConfigureAwait(false))
        {
            taskList = new(this.subscriptions.Values.Select(s => s.DisposeAsync().AsTask()));
        }

        if (this.mqttClient.IsConnected)
        {
            await ((IMqttConnection)this).PublishMessage(
                $"boinc2mqtt/{clientId}/available",
                "offline").ConfigureAwait(false);

            while (this.mqttClient.PendingApplicationMessagesCount > 0)
            {
                await Task.Yield();
            }
        }

        await this.mqttClient.StopAsync(true).ConfigureAwait(false);
        await Task.WhenAll(taskList).ConfigureAwait(false);

        this.LogInformationDisconnected();
    }

    /// <inheritdoc/>
    Task<string> IMqttConnection.GetClientIdentifierAsync(CancellationToken cancellationToken) => this.clientIdentifer.WithCancellation(cancellationToken);

    /// <inheritdoc/>
    async Task<MqttConnection> IAsyncInitialise<MqttConnection>.InitialiseAsync(CancellationToken cancellationToken)
    {
        var clientId = await ((IMqttConnection)this).GetClientIdentifierAsync(cancellationToken).ConfigureAwait(false);

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithConnectionUri(this.options.MQTT.Uri)
            .WithClientId(clientId)
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
            .WithWillTopic($"boinc2mqtt/{clientId}/available")
            .WithWillPayload("offline")
            .WithWillRetain(true)
            .Build();

        var managedMqttClientOptions = this.mqttFactory.CreateManagedMqttClientOptionsBuilder()
            .WithClientOptions(mqttClientOptions)
            .Build();

        this.mqttClient.ApplicationMessageReceivedAsync += this.ApplicationMessageReceivedAsync;

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

        this.mqttClient.ConnectedAsync += ConnectedAsync;

        this.mqttClient.ConnectingFailedAsync += ConnectingFailedAsync;

        await this.mqttClient.StartAsync(managedMqttClientOptions).ConfigureAwait(false);

        await tcs.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

        this.mqttClient.ConnectedAsync -= ConnectedAsync;

        this.mqttClient.ConnectingFailedAsync -= ConnectingFailedAsync;

        await ((IMqttConnection)this).PublishMessage($"boinc2mqtt/{clientId}/available", "online", cancellationToken: cancellationToken).ConfigureAwait(false);

        return this;
    }

    /// <inheritdoc/>
    async Task IMqttConnection.PublishMessage(string topic, string message, CancellationToken cancellationToken)
    {
        var mb = this.mqttFactory.CreateApplicationMessageBuilder()
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(true)
            .WithPayload(message)
            .WithTopic(topic);

        this.LogDebugSendingMessage(topic, message);

        await this.mqttClient.EnqueueAsync(mb.Build()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    async Task IMqttConnection.PublishMessage(string topic, byte[] message, CancellationToken cancellationToken)
    {
        var mb = this.mqttFactory.CreateApplicationMessageBuilder()
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(true)
            .WithPayload(message)
            .WithTopic(topic);

        this.LogDebugSendingMessage(topic, message);

        await this.mqttClient.EnqueueAsync(mb.Build()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    async Task<MqttSubscription> IMqttConnection.SubscribeToTopic(string topic, SubscriptionCallback callback, CancellationToken cancellationToken)
    {
        var subscription = new MqttSubscription(
            topic,
            callback,
            this.UnregisterSubscriptionAsync);

        var alreadySubscribed = false;

        using (var asyncLockHolder = await this.subscriptionsLock.AcquireWriteLockAsync(cancellationToken).ConfigureAwait(false))
        {
            alreadySubscribed = this.subscriptions.ContainsKey(topic);

            this.subscriptions[topic] = subscription;
        }

        if (!alreadySubscribed)
        {
            await this.mqttClient.SubscribeAsync(topic).ConfigureAwait(false);
        }

        return subscription;
    }

    private static string GenerateNewClientIdentifier() => new(RandomNumberGenerator.GetItems<char>(ClientIdentifierCharacters, 23));

    private async Task ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var topic = args.ApplicationMessage.Topic;

            if (!this.subscriptions.TryGetValue(topic, out var subscription))
            {
                this.LogWarningUnhandledTopic(topic);
                return;
            }

            string payload = args.ApplicationMessage.ConvertPayloadToString();

            this.LogDebugMessageRecieved(topic, payload);

            await subscription.SubscriptionCallback(topic, payload).ConfigureAwait(false);

            await args.AcknowledgeAsync(default).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            this.LogErrorProcessingMessage(exception);
            throw;
        }
    }

    private async Task<string> GetOrMakeClientIdentifierAsync(CancellationToken cancellationToken = default)
    {
        if (this.options.MQTT.ClientIdentifier != null)
        {
            return this.options.MQTT.ClientIdentifier;
        }

        var applicationPath = this.hostEnvironment.ContentRootPath;

        var clientIdentifierPath = this.fileSystem.Path.Join(applicationPath, "ClientIdentifier.txt");

        try
        {
            return await this.fileSystem.File.ReadLinesAsync(clientIdentifierPath, cancellationToken).FirstAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (FileNotFoundException)
        {
            var newClientIdentifier = GenerateNewClientIdentifier();

            await this.fileSystem.File.WriteAllTextAsync(clientIdentifierPath, newClientIdentifier, cancellationToken).ConfigureAwait(false);

            return newClientIdentifier;
        }
    }

    [LoggerMessage(EventId = (int)EventIdentifier.DebugMessageRecieved, Level = LogLevel.Debug, Message = "Received \"{payload}\" for topic \"{topic}\"")]
    private partial void LogDebugMessageRecieved(string topic, string payload);

    [LoggerMessage(EventId = (int)EventIdentifier.DebugSendingMessage, Level = LogLevel.Debug, Message = "Sending {payload} to {topic}", SkipEnabledCheck = true)]
    private partial void LogDebugSendingMessage(string topic, string payload);

    // convert payload to string before logging.
    private void LogDebugSendingMessage(string topic, byte[] payload)
    {
        if (this.logger.IsEnabled(LogLevel.Debug))
        {
            this.LogDebugSendingMessage(topic, Encoding.UTF8.GetString(payload, 0, payload.Length));
        }
    }

    [LoggerMessage(EventId = (int)EventIdentifier.ErrorProcessingMessage, Level = LogLevel.Error, Message = "Failed to process a message")]
    private partial void LogErrorProcessingMessage(Exception exception);

    [LoggerMessage(EventId = (int)EventIdentifier.InformationConnectedToMQTTServer, Level = LogLevel.Information, Message = "Connected to MQTT Server")]
    private partial void LogInformationConnected();

    [LoggerMessage(EventId = (int)EventIdentifier.InformationDisconnectedFromMQTTServer, Level = LogLevel.Information, Message = "Disconnected from MQTT Server")]
    private partial void LogInformationDisconnected();

    [LoggerMessage(EventId = (int)EventIdentifier.WarningUnhandledTopic, Level = LogLevel.Warning, Message = "Unhanded topic \"{topic}\"")]
    private partial void LogWarningUnhandledTopic(string topic);

    private async Task UnregisterSubscriptionAsync(MqttSubscription subscription, CancellationToken cancellationToken = default)
    {
        using (var asyncLockHolder = await this.subscriptionsLock.AcquireWriteLockAsync(cancellationToken).ConfigureAwait(false))
        {
            if (!this.subscriptions.TryGetValue(subscription.Topic, out var currentSubscription) || currentSubscription != subscription)
            {
                return;
            }

            this.subscriptions.Remove(subscription.Topic);
        }

        await this.mqttClient.UnsubscribeAsync(subscription.Topic).ConfigureAwait(false);
    }

    /// <summary>
    /// Wrapper for a <see cref="MqttConnection"/> to ensure it is initialised before being used.
    /// </summary>
    /// <param name="mqttConnection2">The <see cref="MqttConnection"/> being wrapped.</param>
    // TODO do away with this wrapper.
    internal class MqttConnectionWrapper(MqttConnection mqttConnection2) : IMqttConnection
    {
        private readonly AsyncLazy<MqttConnection> mqttConnection = new((ct) => (mqttConnection2 as IAsyncInitialise<MqttConnection>).InitialiseAsync(ct));

        /// <inheritdoc/>
        async Task<string> IMqttConnection.GetClientIdentifierAsync(CancellationToken cancellationToken) => await (await this.mqttConnection.WithCancellation(cancellationToken).ConfigureAwait(false) as IMqttConnection).GetClientIdentifierAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        async Task IMqttConnection.PublishMessage(string topic, string message, CancellationToken cancellationToken) => await (await this.mqttConnection.WithCancellation(cancellationToken).ConfigureAwait(false) as IMqttConnection).PublishMessage(topic, message, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        async Task IMqttConnection.PublishMessage(string topic, byte[] message, CancellationToken cancellationToken) => await (await this.mqttConnection.WithCancellation(cancellationToken).ConfigureAwait(false) as IMqttConnection).PublishMessage(topic, message, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        async Task<MqttSubscription> IMqttConnection.SubscribeToTopic(string topic, SubscriptionCallback callback, CancellationToken cancellationToken) => await (await this.mqttConnection.WithCancellation(cancellationToken).ConfigureAwait(false) as IMqttConnection).SubscribeToTopic(topic, callback, cancellationToken).ConfigureAwait(false);
    }
}
