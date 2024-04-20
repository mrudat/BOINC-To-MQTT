// Ignore Spelling: gpu

namespace BOINC_To_MQTT;

internal partial class GPUController(
    ILogger<GPUController> logger,
    IBOINCConnection bOINCClient,
    TimeProvider timeProvider
    ) : AbstractController, IGPUController
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger _logger = logger;

    private AsyncTaskCompletionSource WaitForThrottleChange = new();

    private double newThrottle = 0;

    internal int minimumWorkTime = 60;

    public void SetGPUUsageLimit(double gpuUsageLimit)
    {
        newThrottle = gpuUsageLimit;
    }

    public async Task UpdateThrottle(double throttle, CancellationToken cancellationToken = default)
    {
        newThrottle = throttle;

        if (throttle < 100)
            await WaitForThrottleChange.TrySetResult();
    }

    public new async Task Run(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var throttle = newThrottle;
            if (throttle >= 100)
            {
                await WaitForThrottleChange.Task.WaitAsync(cancellationToken);
                WaitForThrottleChange = new();
                continue;
            }

            var dutyCycle = throttle / 100;

            var cycleLength = Math.Ceiling(minimumWorkTime / (dutyCycle >= 0.5 ? 1 - dutyCycle : dutyCycle));

            var offTime = (int)Math.Ceiling(cycleLength * (1 - dutyCycle));

            await bOINCClient.SetGpuModeAsync(BoincRpc.Mode.Never, TimeSpan.FromSeconds(offTime), cancellationToken);

            LogInformationPausedGUPWorkload(offTime, cycleLength);

            await Task.Delay(TimeSpan.FromSeconds(cycleLength), timeProvider: timeProvider, cancellationToken: cancellationToken);
        }
    }

    [LoggerMessage(LogLevel.Information, Message = "Paused GPU workload for {offTime}/{cycleLength} seconds", EventId = (int)EventIdentifier.PausedGPUWorkload)]
    private partial void LogInformationPausedGUPWorkload(int offTime, double cycleLength);
}
