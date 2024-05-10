using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Mqtt;
using BOINC_To_MQTT.Scaffolding;
using Microsoft.Extensions.Options;

namespace BOINC_To_MQTT.Work;

internal partial class WorkController(
    IOptions<Boinc2MqttOptions> options,
    IMqttConnection mqttClient,
    IBoincConnection bOINCClient
    ) : IScopedHostedService, IRequiresConfiguration
{
    bool allowMoreWork = false;

    private MqttSubscription? subscription = null;

    internal static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IScopedHostedService, WorkController>()
            .AddScoped<IRequiresConfiguration, WorkController>();
    }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var clientId = await bOINCClient.GetHostCrossProjectIdentifierAsync(cancellationToken);

        allowMoreWork = !(await bOINCClient.GetProjectStatusAsync(cancellationToken)).All(project => project.DontRequestMoreWork);

        await PublishAllowMoreWork(cancellationToken);

        subscription = await mqttClient.RegisterSubscription($"boinc2mqtt/{clientId}/moreWork/set", EnableCallback, cancellationToken);
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        if (subscription != null)
            await subscription.DisposeAsync();
        subscription = null;
    }

    async Task IRequiresConfiguration.ConfigureAsync(CancellationToken cancellationToken)
    {
        var clientId = await bOINCClient.GetHostCrossProjectIdentifierAsync(cancellationToken);

        await mqttClient.PublishMessage(
            $"{options.Value.MQTT.DiscoveryPrefix}/button/{clientId}/config",
            """
            {
                o: {
                    name: "BOINC To MQTT",
                    sw: "{ThisAssembly.Git.Commit}",
                    url: "{ThisAssembly.Git.Url}"
                },
                dev: {
                    ids: "{clientId}",
                    name: "BOINC Client"
                },
                a_t: "boinc2mqtt/{clientId}/available",
                cmd_t: "boinc2mqtt/{clientId}/moreWork/set",
                state_t: "boinc2mqtt/{clientId}/moreWork",
                uniq_id: "{clientId}",
                name: "Allow More Work"
            }
            """,
            retain: true,
            cancellationToken: cancellationToken
        );
    }

    private async Task EnableCallback(string _, string payload, CancellationToken cancellationToken)
    {
        if (payload.Equals("OFF"))
        {
            if (allowMoreWork == false)
                return;
            allowMoreWork = false;

            foreach (var project in from project in await bOINCClient.GetProjectStatusAsync(cancellationToken)
                                    where project.DontRequestMoreWork == false
                                    select project)
            {
                await bOINCClient.PerformProjectOperationAsync(project, BoincRpc.ProjectOperation.NoMoreWork, cancellationToken);
            }
        }
        else if (payload.Equals("ON"))
        {
            if (allowMoreWork == true)
                return;
            allowMoreWork = true;

            foreach (var project in from project in await bOINCClient.GetProjectStatusAsync(cancellationToken)
                                    where project.DontRequestMoreWork == true
                                    select project)
            {
                await bOINCClient.PerformProjectOperationAsync(project, BoincRpc.ProjectOperation.AllowMoreWork, cancellationToken);
            }
        }
        else
        {
            return;
        }

        await PublishAllowMoreWork(cancellationToken);
    }
    private async Task PublishAllowMoreWork(CancellationToken cancellationToken = default)
    {
        var clientId = await bOINCClient.GetHostCrossProjectIdentifierAsync(cancellationToken);

        await mqttClient.PublishMessage($"boinc2mqtt/{clientId}/moreWork", allowMoreWork ? "NO" : "YES", cancellationToken: cancellationToken);
    }
}
