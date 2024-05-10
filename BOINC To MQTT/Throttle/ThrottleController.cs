using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Cpu;
using BOINC_To_MQTT.Gpu;
using BOINC_To_MQTT.Mqtt;
using BOINC_To_MQTT.Scaffolding;
using Microsoft.Extensions.Options;

namespace BOINC_To_MQTT.Throttle;

internal partial class ThrottleController(
    IOptions<Boinc2MqttOptions> options,
    ICpuController cpuController,
    IGpuController gpuController,
    IBoincConnection boincConnection,
    IMqttConnection mqttClient
    ) : IScopedHostedService, IRequiresConfiguration
{
    private double cpuUsageLimit = 100;

    private MqttSubscription? subscription = null;

    internal static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IScopedHostedService, ThrottleController>()
            .AddScoped<IRequiresConfiguration, ThrottleController>();
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var clientId = await boincConnection.GetHostCrossProjectIdentifierAsync(cancellationToken);

        cpuUsageLimit = (await boincConnection.GetGlobalPreferencesWorkingAsync(cancellationToken)).CpuUsageLimit;

        cpuController.SetThrottle(cpuUsageLimit);
        gpuController.SetThrottle(cpuUsageLimit);

        await PublishCPUUsageLimit(cancellationToken);

        subscription = await mqttClient.RegisterSubscription($"boinc2mqtt/{clientId}/throttle/set", ThrottleCallback, cancellationToken);
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        if (subscription != null)
            await subscription.DisposeAsync();
        subscription = null;
    }

    async Task IRequiresConfiguration.ConfigureAsync(CancellationToken cancellationToken)
    {
        var clientId = await boincConnection.GetHostCrossProjectIdentifierAsync(cancellationToken);

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
            cpuController.UpdateThrottleAsync(throttle, cancellationToken),
            gpuController.UpdateThrottleAsync(throttle, cancellationToken)
        ]);

        cpuUsageLimit = throttle;

        await PublishCPUUsageLimit(cancellationToken);
    }

    private async Task PublishCPUUsageLimit(CancellationToken cancellationToken = default)
    {
        var clientId = await boincConnection.GetHostCrossProjectIdentifierAsync(cancellationToken);

        await mqttClient.PublishMessage($"boinc2mqtt/{clientId}/throttle", cpuUsageLimit.ToString(), cancellationToken: cancellationToken);
    }
}
