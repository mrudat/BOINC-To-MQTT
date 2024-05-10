// Ignore Spelling: GPU

using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Scaffolding;
using BOINC_To_MQTT.Throttle;

namespace BOINC_To_MQTT.Gpu;

internal partial class GpuController(
    ILogger<GpuController> logger,
    IBoincContext boincContext,
    IBoincConnection bOINCClient,
    TimeProvider timeProvider
    ) : ScopedBackgroundService, IGpuController
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger _logger = logger;

    private AsyncTaskCompletionSource WaitForThrottleChange = new();

    private double newThrottle = 0;

    internal readonly uint minimumWorkTime = boincContext.Options.MinimumGPUWorkTime;

    internal readonly uint minimumSleepTime = boincContext.Options.MinimumGPUSleepTime;

    internal readonly uint maximumCycleTime = boincContext.Options.MaximumGPUCycleTime;

    private readonly double minimumThrottle = 100.0 * boincContext.Options.MinimumGPUWorkTime / boincContext.Options.MaximumGPUCycleTime;

    private readonly double maximumThrottle = 100.0 * (boincContext.Options.MaximumGPUCycleTime - boincContext.Options.MinimumGPUSleepTime) / boincContext.Options.MaximumGPUCycleTime;

    internal static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IScopedHostedService, GpuController>()
            .AddScoped<IGpuController, GpuController>();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var throttle = newThrottle;
            if (throttle >= maximumThrottle)
            {
                LogInformationFullThrottle();
                await WaitForThrottleChange.Task.WaitAsync(cancellationToken);
                WaitForThrottleChange = new();
                continue;
            }

            CalculateCycleTimes(throttle, out var cycleLength, out var offTime);

            await bOINCClient.SetGpuModeAsync(BoincRpc.Mode.Never, TimeSpan.FromSeconds(offTime), cancellationToken);

            LogInformationPausedGUPWorkload(offTime, cycleLength);

            await Task.Delay(TimeSpan.FromSeconds(cycleLength), timeProvider: timeProvider, cancellationToken: cancellationToken);
        }
    }

    void IThrottleable.SetThrottle(double gpuUsageLimit)
    {
        newThrottle = gpuUsageLimit;
    }

    async Task IThrottleable.UpdateThrottleAsync(double throttle, CancellationToken cancellationToken)
    {
        newThrottle = throttle;

        if (throttle < 100)
            await WaitForThrottleChange.TrySetResult();
    }

    internal void CalculateCycleTimes(double throttle, out double cycleLength, out uint offTime)
    {
        var dutyCycle = throttle / 100;

        cycleLength = minimumWorkTime / dutyCycle;

        if (cycleLength > maximumCycleTime)
            cycleLength = maximumCycleTime;

        offTime = (uint)Math.Ceiling(cycleLength * (1 - dutyCycle));

        if (offTime < minimumSleepTime)
            offTime = minimumSleepTime;

        cycleLength = offTime / (1 - dutyCycle);

        if (cycleLength > maximumCycleTime)
            cycleLength = maximumCycleTime;

        if ((cycleLength - offTime) < minimumWorkTime)
            offTime = (uint)(cycleLength - minimumWorkTime);
    }

    [LoggerMessage(LogLevel.Information, Message = "Paused GPU workload for {offTime}/{cycleLength} seconds", EventId = (int)EventIdentifier.PausedGPUWorkload)]
    private partial void LogInformationPausedGUPWorkload(uint offTime, double cycleLength);

    [LoggerMessage(LogLevel.Information, Message = "GPU running at full throttle", EventId = (int)EventIdentifier.FullThrottle)]
    private partial void LogInformationFullThrottle();

}
