using MQTTnet;
using MQTTnet.Client;

namespace BOINC_To_MQTT
{
    internal partial class BOINCWorker(
        Lazy<ClientId> clientId, // TODO AsyncLazy
        ILogger<BOINCWorker> logger,
        IConfiguration configuration,
        MQTTWorker mQTTWorker) : BackgroundService

    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            // TODO read clientId from BOINC data directory
            { 
            }

            // TODO subscribe to control topic
            {
                MqttClientSubscribeOptionsBuilder sb = new();

                MqttClientSubscribeOptions subscribeOptions = new();

                await mQTTWorker.RegisterSubscription(subscribeOptions, Callback);
            }

            // TODO publish configuration data
            {
                MqttApplicationMessageBuilder mb = new();
                mb.WithRetainFlag();
                MqttApplicationMessage configurationMessage = new();
                await mQTTWorker.PublishMessage(configurationMessage, stoppingToken);
            }


            // Do the following forever:

            // TODO control GPU utilisation
            // TODO control CPU utilisation
        }

        private async Task Callback(MqttApplicationMessageReceivedEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
