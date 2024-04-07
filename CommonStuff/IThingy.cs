using MQTTnet;
using MQTTnet.Client;

namespace CommonStuff;

public interface IThingy
{
    public Task PublishMessage(MqttApplicationMessage message, CancellationToken stoppingToken);

    public Task RegisterSubscription(MqttClientSubscribeOptions subscribeOptions, MqttApplicationMessageReceivedEventHandler callback);

    public delegate Task MqttApplicationMessageReceivedEventHandler(MqttApplicationMessageReceivedEventArgs args, CancellationToken cancellationToken = default);
}

