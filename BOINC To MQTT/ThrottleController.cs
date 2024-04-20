using Microsoft.Extensions.Options;

namespace BOINC_To_MQTT;

internal partial class ThrottleController(
    IOptions<BOINC2MQTTWorkerOptions> options,
    ICPUController cPUController,
    IGPUController gPUController,
    IBOINCConnection bOINCClient,
    IMQTTConnection mqttClient
    ) : AbstractController, IThrottleController
{
    double cpuUsageLimit = 100;

    public new async Task SetUp(CancellationToken cancellationToken = default)
    {
        var clientId = await bOINCClient.GetClientIdentifierAsync(cancellationToken);

        cpuUsageLimit = (await bOINCClient.GetGlobalPreferencesWorkingAsync(cancellationToken)).CpuUsageLimit;

        cPUController.SetCPUUsageLimit(cpuUsageLimit);
        gPUController.SetGPUUsageLimit(cpuUsageLimit);

        await PublishCPUUsageLimit(cancellationToken);

        await mqttClient.RegisterSubscription($"boinc2mqtt/{clientId}/throttle/set", ThrottleCallback, cancellationToken);

        await Task.WhenAll([
            cPUController.SetUp(cancellationToken),
            gPUController.SetUp(cancellationToken)
        ]);
    }

    public async Task PublishCPUUsageLimit(CancellationToken cancellationToken = default)
    {
        var clientId = await bOINCClient.GetClientIdentifierAsync(cancellationToken);

        await mqttClient.PublishMessage($"boinc2mqtt/{clientId}/throttle", cpuUsageLimit.ToString(), cancellationToken: cancellationToken);
    }

    public new async Task Configure(CancellationToken cancellationToken = default)
    {
        var clientId = await bOINCClient.GetClientIdentifierAsync(cancellationToken);

        await mqttClient.PublishMessage(
            $"{options.Value.MQTT.DiscoveryPrefix}/number/{clientId}/config",
            """
                {
                    o: {
                        name: "BOINC To MQTT",
                        sw: "{GitVersion}",
                        url: "{GitHubPage}"
                    },
                    dev: {
                        ids: "{clientId}"
                    },
                    a_t: "boinc2mqtt/{clientId}/available",
                    cmd_t: "boinc2mqtt/{clientId}/throttle/set",
                    state_t: "boinc2mqtt/{clientId}/throttle",
                    uniq_id: "{clientId}",
                    name: "CPU/GPU Throttle"
                }
                """,
            retain: true,
            cancellationToken: cancellationToken
        );

        await Task.WhenAll([
            cPUController.Configure(cancellationToken),
            gPUController.Configure(cancellationToken)
        ]);
    }

    public new async Task Run(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll([
            cPUController.Run(cancellationToken),
            gPUController.Run(cancellationToken)
        ]);
    }

    private async Task ThrottleCallback(string _, string payload, CancellationToken cancellationToken)
    {
        if (!double.TryParse(payload, out var bar))
            return;

        var throttle = bar switch
        {
            > 100 => 100,
            < 10 => 10,
            _ => bar,
        };

        await Task.WhenAll([
            cPUController.UpdateThrottle(throttle, cancellationToken),
            gPUController.UpdateThrottle(throttle, cancellationToken)
        ]);

        cpuUsageLimit = throttle;

        await PublishCPUUsageLimit(cancellationToken);
    }
}
