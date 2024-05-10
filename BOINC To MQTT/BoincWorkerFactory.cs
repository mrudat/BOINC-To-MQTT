using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Mqtt;
using BOINC_To_MQTT.Scaffolding;
using DotNext.Threading;
using Microsoft.Extensions.Options;

namespace BOINC_To_MQTT;

internal partial class BoincWorkerFactory(
    ILogger<BoincWorkerFactory> logger,
    IMqttConnection mqttClient,
    IOptionsMonitor<Boinc2MqttOptions> options,
    IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger _logger = logger;

    private Dictionary<CommonBoincOptions, AsyncServiceScope> scopes = [];

    private Dictionary<CommonBoincOptions, AsyncServiceScope> oldScopes = [];

    private readonly AsyncReaderWriterLock scopesLock = new();

    private IDisposable? optionsChangeListenerRegistration = null;

    private bool configurationRequested = false;

    private MqttSubscription? subscription = null;

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        subscription = await mqttClient.RegisterSubscription($"{options.CurrentValue.MQTT.DiscoveryPrefix}/status", ConfigureCallback, cancellationToken);

        await UpdateOptionsAsync(options.CurrentValue, cancellationToken);

        optionsChangeListenerRegistration = options.OnChange(OptionsChangeListenerAsync(cancellationToken));
    }

    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(scopes.Values.SelectMany(scope => scope.ServiceProvider.GetServices<IScopedHostedService>().Select(shs => shs.StopAsync(cancellationToken))));

        if (optionsChangeListenerRegistration != null)
        {
            optionsChangeListenerRegistration.Dispose();
            optionsChangeListenerRegistration = null;
        }

        if (subscription != null)
        {
            await subscription.DisposeAsync();
            subscription = null;
        }

        using var _ = await scopesLock.AcquireWriteLockAsync(cancellationToken);

        await Task.WhenAll(scopes.Values.Select(scope => scope.DisposeAsync().AsTask()));

        scopes.Clear();
    }

    private async Task ConfigureCallback(string topic, string payload, CancellationToken cancellationToken)
    {
        if (payload != "online")
            return;
        configurationRequested = true;

        var configureTasks = new List<Task>(scopes.Count);

        using (var _ = await scopesLock.AcquireReadLockAsync(cancellationToken))
        {
            foreach (var scope in scopes.Values)
                configureTasks.AddRange(scope.ServiceProvider.GetServices<IRequiresConfiguration>().Select(rc => rc.ConfigureAsync(cancellationToken)));
        }

        await Task.WhenAll(configureTasks);
    }

    private Action<Boinc2MqttOptions> OptionsChangeListenerAsync(CancellationToken cancellationToken = default)
    {
        return (options) =>
        {
            Task.Run(async () => await UpdateOptionsAsync(options, cancellationToken), cancellationToken);
        };
    }

    private async Task UpdateOptionsAsync(Boinc2MqttOptions options, CancellationToken cancellationToken = default)
    {
        using var _ = await scopesLock.AcquireWriteLockAsync(cancellationToken);

        var connectionCount = options.Local.Length + options.Remote.Length;

        var newScopes = oldScopes;
        newScopes.EnsureCapacity(connectionCount);

        var configureTasks = new List<Task>(connectionCount * 2);

        foreach (var boincOptions in options.Local.Concat<CommonBoincOptions>(options.Remote))
        {
            if (scopes.TryGetValue(boincOptions, out var scope))
            {
                newScopes[boincOptions] = scope;
                continue;
            }

            scope = serviceScopeFactory.CreateAsyncScope();
            try
            {
                var services = scope.ServiceProvider;

                var scopeOptions = services.GetRequiredService<IBoincContext>();
                ((BoincContext)scopeOptions).BoincOptions = boincOptions;

                await Task.WhenAll(scope.ServiceProvider
                    .GetServices<IScopedHostedService>()
                    .Select(hs => hs.StartAsync(cancellationToken)));

                if (configurationRequested)
                    configureTasks.AddRange(scope.ServiceProvider
                        .GetServices<IRequiresConfiguration>()
                        .Select(rc => rc.ConfigureAsync(cancellationToken)));
            }
            catch (Exception e)
            {
                LogErrorFailedToStartBoincWorker(e);
                await scope.DisposeAsync();
#if DEBUG
                throw;
#else
                continue;
#endif
            }

            newScopes[boincOptions] = scope;
        }

        await Task.WhenAll(configureTasks);

        await Task.WhenAll(scopes.Values.Except(newScopes.Values).Select(scope => scope.DisposeAsync().AsTask()));

        oldScopes = scopes;

        scopes = newScopes;

        oldScopes.Clear();
    }

    [LoggerMessage(EventId = (int)EventIdentifier.ErrorFailedToConnectoToBoinc, Level = LogLevel.Error, Message = "Failed to start BOINC worker")]

    private partial void LogErrorFailedToStartBoincWorker(Exception e);
}
