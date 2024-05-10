
using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Scaffolding;
using BOINC_To_MQTT.Throttle;

namespace BOINC_To_MQTT.Cpu;

internal partial class CpuController(
    ILogger<CpuController> logger,
    IBoincConnection boincConnection,
    TimeProvider timeProvider
    ) : ScopedBackgroundService, ICpuController
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger _logger = logger;

    private AsyncTaskCompletionSource ThrottleHasChanged = new();

    private double newThrottle = 0;

    internal static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IScopedHostedService, CpuController>()
            .AddScoped<ICpuController, CpuController>();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var throttle = newThrottle;

        while (!cancellationToken.IsCancellationRequested)
        {
            await ApplyCPUThrottle(throttle, cancellationToken);

            LogInformationNewCPUThrottleSetting(throttle);

            await Task.Delay(TimeSpan.FromSeconds(30), timeProvider: timeProvider, cancellationToken: cancellationToken);

            await ThrottleHasChanged.Task.WaitAsync(cancellationToken);
            throttle = newThrottle;
            ThrottleHasChanged = new();
        }
    }

    void IThrottleable.SetThrottle(double cpuUsageLimit)
    {
        newThrottle = cpuUsageLimit;
    }

    async Task IThrottleable.UpdateThrottleAsync(double throttle, CancellationToken cancellationToken)
    {
        newThrottle = throttle;

        await ThrottleHasChanged.TrySetResult();
    }

    internal async Task ApplyCPUThrottle(double throttle, CancellationToken cancellationToken = default)
    {
        var globalPreferencesOverride = await boincConnection.GetGlobalPreferencesOverrideAsync(cancellationToken);

        globalPreferencesOverride.SetElementValue("cpu_usage_limit", throttle.ToString());

        await boincConnection.SetGlobalPreferencesOverrideAsync(globalPreferencesOverride, cancellationToken);

        await boincConnection.ReadGlobalPreferencesOverrideAsync(cancellationToken);
    }

    [LoggerMessage(LogLevel.Information, Message = "New CPU throttle setting: {throttle}", EventId = (int)EventIdentifier.NewCPUThrottleSetting)]
    private partial void LogInformationNewCPUThrottleSetting(double throttle);

}
