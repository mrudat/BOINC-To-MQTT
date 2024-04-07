using MQTTnet.Client;
using static CommonStuff.IThingy;

namespace MQTTWorker;

public partial class MQTTWorker
{
    internal class Subscription(MqttClientSubscribeOptions subscribeOptions, MqttApplicationMessageReceivedEventHandler handleAsync)
    {
        internal readonly MqttClientSubscribeOptions subscribeOptions = subscribeOptions;

        internal readonly MqttApplicationMessageReceivedEventHandler HandleAsync = handleAsync;
    }
}
