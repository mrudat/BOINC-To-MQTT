// Ignore Spelling: Rpc Gpu BOINC

using BoincRpc;
using DotNext.Threading;
using System.IO.Abstractions;
using System.Xml.Linq;

namespace BOINC_To_MQTT.Boinc;

internal class BoincConnection(
    IBoincContext boincContext,
    IFileSystem fileSystem
    ) : AsyncLazy<RpcClient>((ct) => ConnectToBoinc(boincContext, fileSystem, ct)), IBoincConnection, IDisposable
{
    public const ushort BoincRpcPort = 31416;

    private string? HostCrossProjectIdentifier;

    private bool disposedValue;

    internal static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IBoincConnection, BoincConnection>();
    }

    private static async Task<RpcClient> ConnectToBoinc(
        IBoincContext boincContext,
        IFileSystem fileSystem,
        CancellationToken cancellationToken)
    {
        var rpcClient = new RpcClient();

        await rpcClient.ConnectAsync(boincContext.Options.GetHostName(), boincContext.Options.GetPort()).WaitAsync(cancellationToken);

        var authorized = await rpcClient.AuthorizeAsync(await boincContext.Options.GetGuiRpcKeyAsync(fileSystem, cancellationToken));

        if (!authorized)
            throw new AuthorisationFailedException(boincContext);

        return rpcClient;
    }

    async Task<string> IBoincConnection.GetHostCrossProjectIdentifierAsync(CancellationToken cancellationToken)
    {
        HostCrossProjectIdentifier ??= (await ((IBoincConnection)this).GetHostInfoAsync(cancellationToken)).HostCPID;
        return HostCrossProjectIdentifier;
    }

    async Task<XElement> IBoincConnection.GetGlobalPreferencesOverrideAsync(CancellationToken cancellationToken) => await (await WithCancellation(cancellationToken)).GetGlobalPreferencesOverrideAsync().WaitAsync(cancellationToken);

    async Task<GlobalPreferences> IBoincConnection.GetGlobalPreferencesWorkingAsync(CancellationToken cancellationToken) => await (await WithCancellation(cancellationToken)).GetGlobalPreferencesWorkingAsync().WaitAsync(cancellationToken);

    async Task<Project[]> IBoincConnection.GetProjectStatusAsync(CancellationToken cancellationToken) => await (await WithCancellation(cancellationToken)).GetProjectStatusAsync().WaitAsync(cancellationToken);

    async Task IBoincConnection.PerformProjectOperationAsync(Project project, ProjectOperation operation, CancellationToken cancellationToken) => await (await WithCancellation(cancellationToken)).PerformProjectOperationAsync(project, operation).WaitAsync(cancellationToken);

    async Task IBoincConnection.ReadGlobalPreferencesOverrideAsync(CancellationToken cancellationToken) => await (await WithCancellation(cancellationToken)).ReadGlobalPreferencesOverrideAsync().WaitAsync(cancellationToken);

    async Task IBoincConnection.SetGlobalPreferencesOverrideAsync(XElement? globalPreferencesOverride, CancellationToken cancellationToken) => await (await WithCancellation(cancellationToken)).SetGlobalPreferencesOverrideAsync(globalPreferencesOverride).WaitAsync(cancellationToken);

    async Task IBoincConnection.SetGpuModeAsync(Mode mode, TimeSpan timeSpan, CancellationToken cancellationToken) => await (await WithCancellation(cancellationToken)).SetGpuModeAsync(mode, timeSpan).WaitAsync(cancellationToken);

    async Task<HostInfo> IBoincConnection.GetHostInfoAsync(CancellationToken cancellationToken) => await (await WithCancellation(cancellationToken)).GetHostInfoAsync().WaitAsync(cancellationToken);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (IsValueCreated)
                {
                    Value!.Value.Value.Dispose();
                }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
