using Microsoft.Extensions.Options;

namespace BOINC_To_MQTT;

internal class BOINC2MQTTWorker(
    IMQTTConnection mqttClient,
    IOptions<BOINC2MQTTWorkerOptions> options,
    IThrottleController throttleController,
    IWorkController workController) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await mqttClient.RegisterSubscription($"{options.Value.MQTT.DiscoveryPrefix}/status", ConfigureCallback, cancellationToken);

        await Task.WhenAll([
            throttleController.SetUp(cancellationToken),
            workController.SetUp(cancellationToken)
        ]);

        await Task.WhenAll([
            throttleController.Run(cancellationToken),
            workController.Run(cancellationToken)
        ]);
    }

    internal async Task ConfigureCallback(string topic, string payload, CancellationToken cancellationToken)
    {
        if (payload == "online")
        {
            await Task.WhenAll([
                throttleController.Configure(cancellationToken),
                workController.Configure(cancellationToken)
            ]);
        }
    }
}
