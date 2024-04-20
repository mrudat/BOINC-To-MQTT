namespace BOINC_To_MQTT;

internal partial class CPUController(
    ILogger<CPUController> logger,
    IBOINCConnection bOINCConnection,
    TimeProvider timeProvider
    ) : AbstractController, ICPUController
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger _logger = logger;

    private AsyncTaskCompletionSource ThrottleHasChanged = new();

    private double newThrottle = 0;

    public void SetCPUUsageLimit(double cpuUsageLimit)
    {
        newThrottle = cpuUsageLimit;
    }

    public async Task UpdateThrottle(double throttle, CancellationToken cancellationToken = default)
    {
        newThrottle = throttle;

        await ThrottleHasChanged.TrySetResult();
    }

    public new async Task Run(CancellationToken cancellationToken)
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

    internal async Task ApplyCPUThrottle(double throttle, CancellationToken cancellationToken = default)
    {
        var globalPreferencesOverride = await bOINCConnection.GetGlobalPreferencesOverrideAsync(cancellationToken);

        globalPreferencesOverride.SetElementValue("cpu_usage_limit", throttle.ToString());

        await bOINCConnection.SetGlobalPreferencesOverrideAsync(globalPreferencesOverride, cancellationToken);

        await bOINCConnection.ReadGlobalPreferencesOverrideAsync(cancellationToken);
    }

    [LoggerMessage(LogLevel.Information, Message = "New CPU throttle setting: {throttle}", EventId = (int)EventIdentifier.NewCPUThrottleSetting)]
    private partial void LogInformationNewCPUThrottleSetting(double throttle);
}
