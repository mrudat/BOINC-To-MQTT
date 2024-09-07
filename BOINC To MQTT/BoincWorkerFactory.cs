// <copyright file="BoincWorkerFactory.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT;

using System.Collections.Frozen;
using System.Threading;
using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Mqtt;
using BOINC_To_MQTT.Scaffolding;
using DotNext.Threading;
using Microsoft.Extensions.Options;

/// <summary>
/// Controls the lifetime of a BOINC worker for each configured <see cref="Boinc2MqttOptions.Local"/> or <see cref="Boinc2MqttOptions.Remote"/> BOINC client.
/// </summary>
internal partial class BoincWorkerFactory(
    ILogger<BoincWorkerFactory> logger,
    IMqttConnection mqttClient,
    IOptions<Boinc2MqttOptions> options,
    IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    private readonly AsyncLazy<FrozenDictionary<CommonBoincOptions, AsyncServiceScope>> lazyWorkers = new((ct) => CreateWorkers(options.Value, serviceScopeFactory, logger, ct));

    private MqttSubscription? subscription = null;

    /// <inheritdoc/>
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        this.subscription = await mqttClient.SubscribeToTopic($"{options.Value.MQTT.DiscoveryPrefix}/status", this.ConfigureCallback, cancellationToken).ConfigureAwait(false);

        await this.lazyWorkers.WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        var workerScopes = this.lazyWorkers.IsValueCreated ? await this.lazyWorkers.WithCancellation(cancellationToken).ConfigureAwait(false) : null;

        if (workerScopes != null)
        {
            // stop any child (IScoped)HostedServices
            await Task.WhenAll(workerScopes.Values.SelectMany(scope => scope.ServiceProvider.GetServices<IScopedHostedService>().Select(shs => shs.StopAsync(cancellationToken)))).ConfigureAwait(false);
        }

        if (this.subscription != null)
        {
            await this.subscription.DisposeAsync();
            this.subscription = null;
        }

        if (workerScopes != null)
        {
            await Task.WhenAll(workerScopes.Values.Select(scope => scope.DisposeAsync().AsTask())).ConfigureAwait(false);
        }
    }

    private static async Task<FrozenDictionary<CommonBoincOptions, AsyncServiceScope>> CreateWorkers(
        Boinc2MqttOptions options,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BoincWorkerFactory> logger,
        CancellationToken cancellationToken)
    {
        var connectionCount = options.Local.Count + options.Remote.Count;

        var workerScopes = new Dictionary<CommonBoincOptions, AsyncServiceScope>(connectionCount);

        // TODO create multiple sub-applications instead?
        foreach (var boincOptions in options.Local.Concat<CommonBoincOptions>(options.Remote))
        {
            var workerScope = serviceScopeFactory.CreateAsyncScope();
            try
            {
                var services = workerScope.ServiceProvider;

                var scopeOptions = services.GetRequiredService<IBoincContext>();

                // Pass boincOptions to the worker via IBoincContext
                ((BoincContext)scopeOptions).BoincOptions = boincOptions;

                // start any (IScoped)HostedServices belonging to this worker.
                await Task.WhenAll(workerScope.ServiceProvider
                    .GetServices<IScopedHostedService>()
                    .Select(hs => hs.StartAsync(cancellationToken))).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                LogErrorFailedToStartBoincWorker(logger, exception);
                await workerScope.DisposeAsync().ConfigureAwait(false);
#if DEBUG
                throw;
#else
                continue;
#endif
            }

            workerScopes[boincOptions] = workerScope;
        }

        if (workerScopes.Count == 0)
        {
            throw new InvalidOperationException("No BOINC workers were successfully created.");
        }

        return workerScopes.ToFrozenDictionary();
    }

    [LoggerMessage(EventId = (int)EventIdentifier.ErrorFailedToStartBoincWorker, Level = LogLevel.Error, Message = "Failed to start BOINC worker")]
    private static partial void LogErrorFailedToStartBoincWorker(ILogger<BoincWorkerFactory> logger, Exception e);

    private async Task ConfigureCallback(string topic, string payload, CancellationToken cancellationToken)
    {
        if (payload != "online")
        {
            return;
        }

        var workerScopes = await this.lazyWorkers.WithCancellation(cancellationToken).ConfigureAwait(false);

        var configureTasks = new List<Task>(workerScopes.Count);

        foreach (var scope in workerScopes.Values)
        {
            configureTasks.AddRange(scope.ServiceProvider.GetServices<IRequiresConfiguration>().Select(rc => rc.ConfigureAsync(cancellationToken)));
        }

        await Task.WhenAll(configureTasks).ConfigureAwait(false);
    }
}
