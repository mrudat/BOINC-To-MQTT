using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace BOINCWorker;

internal partial class GPUController(
    ILogger<BOINCWorker> logger,
    IOptions<BOINCWorkerOptions> options,
    IFileSystem fileSystem,
    ReadOldThrottleSetting readOldThrottleSetting
    )
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger _logger = logger;

    private readonly RunBOINCCmdPauseGPUWork PauseGPUWork = new(fileSystem, options.Value.BinaryPath);

    private TaskCompletionSource WaitForThrottleChange = new();

    private double newThrottle = 0;

    internal async Task ThrottleGUPUtilisaion(CancellationToken cancellationToken = default)
    {
        double throttle = await readOldThrottleSetting.FetchAsync(cancellationToken);
        newThrottle = throttle;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (throttle >= 100)
            {
                await WaitForThrottleChange.Task.WaitAsync(cancellationToken);
                WaitForThrottleChange = new();
                continue;
            }

            throttle = newThrottle;

            var dutyCycle = throttle / 100;

            var cycleLength = Math.Ceiling(10 / (dutyCycle >= 0.5 ? 1 - dutyCycle : dutyCycle));

            var offTime = (int)Math.Ceiling(cycleLength * (1 - dutyCycle));

            await PauseGPUWork.Run(offTime, cancellationToken);

            LogInformationPausedGUPWorkload(offTime, cycleLength);

            await Task.Delay(TimeSpan.FromSeconds(cycleLength), cancellationToken);
        }
    }

    internal async Task UpdateThrottle(double throttle, CancellationToken cancellationToken = default)
    {
        newThrottle = throttle;

        if (throttle < 100)
            WaitForThrottleChange.TrySetResult();

        await Task.Yield();
    }

    [LoggerMessage(LogLevel.Information, Message = "Paused GPU workload for {offTime}/{cycleLength} seconds", EventId = (int)EventIdentifier.PausedGPUWorkload)]
    private partial void LogInformationPausedGUPWorkload(int offTime, double cycleLength);
}
