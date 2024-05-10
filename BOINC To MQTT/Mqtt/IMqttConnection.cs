// Ignore Spelling: BOINC MQTT

namespace BOINC_To_MQTT.Mqtt;

internal interface IMqttConnection
{
    internal Task PublishMessage(string topic, string payload, bool retain = false, CancellationToken cancellationToken = default);

    internal Task<MqttSubscription> RegisterSubscription(string topic, SubscriptionCallback callback, CancellationToken cancellationToken = default);
}