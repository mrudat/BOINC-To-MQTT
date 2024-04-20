// Ignore Spelling: Rpc Gpu

using BoincRpc;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using System.Xml.Linq;

namespace BOINC_To_MQTT;

internal class BOINCConnection(
    IOptions<BOINC2MQTTWorkerOptions> options,
    IFileSystem fileSystem
    ) : IHostedService, IBOINCConnection
{
    private RpcClient? rpcClient = null;

    private static string? ClientIdentifier = null;

    // TODO should this be here?
    public async Task<string> GetClientIdentifierAsync(CancellationToken cancellationToken = default)
    {
        if (ClientIdentifier == null)
        {
            if (options.Value.MQTT.ClientIdentifier != null)
            {
                ClientIdentifier = options.Value.MQTT.ClientIdentifier;
            }
            else
            {
                HostInfo hostInfo = await GetHostInfoAsync(cancellationToken);
                ClientIdentifier = hostInfo.HostCPID;
            }
        }

        return ClientIdentifier;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await GetRpcClient(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (rpcClient is not null)
        {
            await Task.Run(() => rpcClient.Dispose(), cancellationToken);
            rpcClient = null;
        }
    }

    public async Task<RpcClient> GetRpcClient(CancellationToken cancellationToken)
    {
        if (rpcClient == null || !rpcClient.Connected)
        {
            if (rpcClient is not null)
            {
                rpcClient.Dispose();
                rpcClient = null;
            }
            rpcClient = new();

            await rpcClient.ConnectAsync("localhost", 31416).WaitAsync(cancellationToken);

            var authorized = await rpcClient.AuthorizeAsync(await GetRPCKey(cancellationToken));

            if (!authorized)
                throw new InvalidOperationException("TODO throw the right kind of exception.");
        }
        return rpcClient;
    }

    internal async Task<string> GetRPCKey(CancellationToken cancellationToken = default) => await fileSystem.File.ReadAllTextAsync(fileSystem.Path.Combine(options.Value.BOINC.DataPath, "gui_rpc_auth.cfg"), cancellationToken);

    public async Task<XElement> GetGlobalPreferencesOverrideAsync(CancellationToken cancellationToken)
    {
        var rpcClient = await GetRpcClient(cancellationToken);

        return await rpcClient.GetGlobalPreferencesOverrideAsync().WaitAsync(cancellationToken);
    }

    public async Task<GlobalPreferences> GetGlobalPreferencesWorkingAsync(CancellationToken cancellationToken)
    {
        var rpcClient = await GetRpcClient(cancellationToken);

        return await rpcClient.GetGlobalPreferencesWorkingAsync().WaitAsync(cancellationToken);
    }

    public async Task<Project[]> GetProjectStatusAsync(CancellationToken cancellationToken)
    {
        var rpcClient = await GetRpcClient(cancellationToken);

        return await rpcClient.GetProjectStatusAsync().WaitAsync(cancellationToken);
    }

    public async Task PerformProjectOperationAsync(Project project, ProjectOperation operation, CancellationToken cancellationToken)
    {
        var rpcClient = await GetRpcClient(cancellationToken);

        await rpcClient.PerformProjectOperationAsync(project, operation).WaitAsync(cancellationToken);
    }

    public async Task ReadGlobalPreferencesOverrideAsync(CancellationToken cancellationToken)
    {
        var rpcClient = await GetRpcClient(cancellationToken);

        await rpcClient.ReadGlobalPreferencesOverrideAsync().WaitAsync(cancellationToken);
    }

    public async Task SetGlobalPreferencesOverrideAsync(XElement? globalPreferencesOverride, CancellationToken cancellationToken)
    {
        var rpcClient = await GetRpcClient(cancellationToken);

        await rpcClient.SetGlobalPreferencesOverrideAsync(globalPreferencesOverride).WaitAsync(cancellationToken);
    }

    public async Task SetGpuModeAsync(Mode mode, TimeSpan timeSpan, CancellationToken cancellationToken)
    {
        var rpcClient = await GetRpcClient(cancellationToken);

        await rpcClient.SetGpuModeAsync(mode, timeSpan).WaitAsync(cancellationToken);
    }

    public async Task<HostInfo> GetHostInfoAsync(CancellationToken cancellationToken)
    {
        var rpcClient = await GetRpcClient(cancellationToken);

        return await rpcClient.GetHostInfoAsync().WaitAsync(cancellationToken);
    }
}
