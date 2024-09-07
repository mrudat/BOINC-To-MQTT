// <copyright file="BoincConnection.cs" company="Martin Rudat">
// BOINC To MQTT - Exposes some BOINC controls via MQTT for integration with Home Assistant.
// Copyright (C) 2024  Martin Rudat
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>

namespace BOINC_To_MQTT.Boinc;

using System.IO.Abstractions;
using System.Threading;
using System.Xml.Linq;
using BOINC_To_MQTT.Scaffolding;
using BoincRpc;
using DotNext.Threading;
using Polly;
using Polly.Retry;

/// <summary>
/// Holds on to an open RPC connection to the target BOINC client.
/// </summary>
internal class BoincConnection : IBoincConnection, IDisposable, IHostApplicationBuilderConfiguration
{
    /// <summary>
    /// The default BOINC GUI RPC port.
    /// </summary>
    public const ushort BoincRpcPort = 31416;
    private readonly IFileSystem fileSystem;
    private readonly IBoincContext boincContext;
    private readonly ResiliencePipeline connectRetry;

    // FIXME attempt to re-connect on connection failure?
    private readonly AsyncLazy<RpcClient> boincConnection;

    private readonly AsyncLazy<Device> device;

    private bool disposedValue;

    private string? hostCrossProjectIdentifier;

    private RpcClient? rpcClient;

    private readonly AsyncLock rpcClientLock = default;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoincConnection"/> class.
    /// </summary>
    /// <param name="boincContext">An <see cref="IBoincContext"/> for the target BOINC client.</param>
    /// <param name="fileSystem">An optional <see cref="IFileSystem"/>.</param>
    public BoincConnection(
        IBoincContext boincContext,
        IFileSystem? fileSystem)
    {
        this.fileSystem = fileSystem ?? new FileSystem();
        this.boincContext = boincContext;

        this.connectRetry = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions()
            {
                BackoffType = DelayBackoffType.Exponential,
            })
            .Build();

        this.boincConnection = new((ct) => ConnectToBoinc(boincContext, this.fileSystem, ct));

        this.device = new((ct) => MakeDevice(this, boincContext, ct));

        _ = this.GetRpcClientAsync();
    }

    private async Task<RpcClient> GetRpcClientAsync(CancellationToken cancellationToken = default)
    {
        using var lockHolder = await this.rpcClientLock.AcquireAsync(cancellationToken);

        if (this.rpcClient?.Connected == true)
        {
            return this.rpcClient;
        }

        this.rpcClient?.Dispose();

        await this.connectRetry.ExecuteAsync(
            async cancellationToken2 =>
            {
                this.rpcClient = await this.ConnectToBoinc2(cancellationToken2);
            },
            cancellationToken);

        // TODO there's presumably a better option.
        return this.rpcClient!;
    }

    private async Task<RpcClient> ConnectToBoinc2(CancellationToken cancellationToken = default)
    {
        var newRpcClient = new RpcClient();
        try
        {
            await newRpcClient.ConnectAsync(this.boincContext.Options.GetHostName(), this.boincContext.Options.GetPort()).WaitAsync(cancellationToken).ConfigureAwait(false);

            if (await newRpcClient.AuthorizeAsync(await this.boincContext.Options.GetGuiRpcKeyAsync(this.fileSystem, cancellationToken).ConfigureAwait(false)))
            {
                return newRpcClient;
            }

            throw new AuthorisationFailedException(this.boincContext);
        }
        catch (Exception)
        {
            newRpcClient.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IBoincConnection, BoincConnection>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    Task<Device> IBoincConnection.GetDeviceAsync(CancellationToken cancellationToken) => this.device.WithCancellation(cancellationToken);

    /// <inheritdoc/>
    async Task<XElement> IBoincConnection.GetGlobalPreferencesOverrideAsync(CancellationToken cancellationToken) => await (await this.boincConnection.WithCancellation(cancellationToken).ConfigureAwait(false)).GetGlobalPreferencesOverrideAsync().WaitAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    async Task<GlobalPreferences> IBoincConnection.GetGlobalPreferencesWorkingAsync(CancellationToken cancellationToken) => await (await this.boincConnection.WithCancellation(cancellationToken).ConfigureAwait(false)).GetGlobalPreferencesWorkingAsync().WaitAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    async Task<string> IBoincConnection.GetHostCrossProjectIdentifierAsync(CancellationToken cancellationToken)
    {
        this.hostCrossProjectIdentifier ??= (await ((IBoincConnection)this).GetHostInfoAsync(cancellationToken).ConfigureAwait(false)).HostCPID;
        return this.hostCrossProjectIdentifier;
    }

    /// <inheritdoc/>
    async Task<HostInfo> IBoincConnection.GetHostInfoAsync(CancellationToken cancellationToken) => await (await this.boincConnection.WithCancellation(cancellationToken).ConfigureAwait(false)).GetHostInfoAsync().WaitAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    async Task<Project[]> IBoincConnection.GetProjectStatusAsync(CancellationToken cancellationToken) => await (await this.boincConnection.WithCancellation(cancellationToken).ConfigureAwait(false)).GetProjectStatusAsync().WaitAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    async Task IBoincConnection.PerformProjectOperationAsync(Project project, ProjectOperation operation, CancellationToken cancellationToken) => await (await this.boincConnection.WithCancellation(cancellationToken).ConfigureAwait(false)).PerformProjectOperationAsync(project, operation).WaitAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    async Task IBoincConnection.ReadGlobalPreferencesOverrideAsync(CancellationToken cancellationToken) => await (await this.boincConnection.WithCancellation(cancellationToken).ConfigureAwait(false)).ReadGlobalPreferencesOverrideAsync().WaitAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    async Task IBoincConnection.SetGlobalPreferencesOverrideAsync(XElement? globalPreferencesOverride, CancellationToken cancellationToken) => await (await this.boincConnection.WithCancellation(cancellationToken).ConfigureAwait(false)).SetGlobalPreferencesOverrideAsync(globalPreferencesOverride).WaitAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    async Task IBoincConnection.SetGpuModeAsync(Mode mode, TimeSpan timeSpan, CancellationToken cancellationToken) => await (await this.boincConnection.WithCancellation(cancellationToken).ConfigureAwait(false)).SetGpuModeAsync(mode, timeSpan).WaitAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing && this.boincConnection.IsValueCreated)
            {
                this.boincConnection.Value!.Value.Value.Dispose();
            }

            this.disposedValue = true;
        }
    }

    private static async Task<RpcClient> ConnectToBoinc(
        IBoincContext boincContext,
        IFileSystem fileSystem,
        CancellationToken cancellationToken)
    {
        var rpcClient = new RpcClient();

        await rpcClient.ConnectAsync(boincContext.Options.GetHostName(), boincContext.Options.GetPort()).WaitAsync(cancellationToken).ConfigureAwait(false);

        var authorized = await rpcClient.AuthorizeAsync(await boincContext.Options.GetGuiRpcKeyAsync(fileSystem, cancellationToken).ConfigureAwait(false));

        if (!authorized)
        {
            throw new AuthorisationFailedException(boincContext);
        }

        return rpcClient;
    }

    private static async Task<Device> MakeDevice(BoincConnection boincConnection, IBoincContext boincContext, CancellationToken cancellationToken)
    {
        var clientId = await ((IBoincConnection)boincConnection).GetHostCrossProjectIdentifierAsync(cancellationToken).ConfigureAwait(false);

        // TODO ConfigurationUrl = account manager
        // TODO SoftwareVersion available via API call?
        // TODO Manufacturer = Berkeley?
        // TODO ViaDevice = host where this program is running.
        var device = new Device()
        {
            Identifiers = [clientId],
            Name = boincContext.GetName(),
        };

        return device;
    }
}
