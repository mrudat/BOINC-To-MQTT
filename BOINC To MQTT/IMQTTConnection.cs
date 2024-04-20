// Ignore Spelling: BOINC MQTT

namespace BOINC_To_MQTT;

internal interface IMQTTConnection
{
    Task PublishMessage(string topic, string payload, bool retain = false, CancellationToken cancellationToken = default);
    Task RegisterSubscription(string topic, SubscriptionCallback callback, CancellationToken cancellationToken = default);
}