// Ignore Spelling: BOINC MQTT

using MQTTnet.Client;

namespace BOINC_To_MQTT;

internal record Subscription
{
    internal MqttClientSubscribeOptions subscribeOptions;
    internal SubscriptionCallback callback;

    public Subscription(MqttClientSubscribeOptions mqttClientSubscribeOptions, SubscriptionCallback callback)
    {
        this.subscribeOptions = mqttClientSubscribeOptions;
        this.callback = callback;
    }
}
