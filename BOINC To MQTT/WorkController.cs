using Microsoft.Extensions.Options;

namespace BOINC_To_MQTT;

internal partial class WorkController(
    IOptions<BOINC2MQTTWorkerOptions> options,
    IMQTTConnection mqttClient,
    IBOINCConnection bOINCClient
   ) : AbstractController, IWorkController
{
    bool allowMoreWork = false;

    public new async Task SetUp(CancellationToken cancellationToken = default)
    {
        var clientId = await bOINCClient.GetClientIdentifierAsync(cancellationToken);

        allowMoreWork = !(await bOINCClient.GetProjectStatusAsync(cancellationToken)).All(project => project.DontRequestMoreWork);

        await PublishAllowMoreWork(cancellationToken);

        await mqttClient.RegisterSubscription($"boinc2mqtt/{clientId}/moreWork/set", EnableCallback, cancellationToken);
    }

    public async Task PublishAllowMoreWork(CancellationToken cancellationToken = default)
    {
        var clientId = await bOINCClient.GetClientIdentifierAsync(cancellationToken);

        await mqttClient.PublishMessage($"boinc2mqtt/{clientId}/moreWork", allowMoreWork ? "NO" : "YES", cancellationToken: cancellationToken);
    }

    public new async Task Configure(CancellationToken cancellationToken = default)
    {
        var clientId = await bOINCClient.GetClientIdentifierAsync(cancellationToken);

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
}
