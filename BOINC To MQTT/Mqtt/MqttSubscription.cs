// Ignore Spelling: BOINC MQTT unregister

namespace BOINC_To_MQTT.Mqtt;

internal record MqttSubscription(
    string Topic,
    SubscriptionCallback SubscriptionCallback,
    UnubscribeCallback UnsubscribeCallback) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await UnsubscribeCallback(this);
    }
}

internal delegate Task SubscriptionCallback(string topic, string payload, CancellationToken cancellationToken = default);

internal delegate Task UnubscribeCallback(MqttSubscription subscriptions, CancellationToken cancellationToken = default);
