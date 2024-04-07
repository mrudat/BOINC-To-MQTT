using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BOINCWorker;

internal partial class CPUController(
    ILogger<CPUController> logger,
    IOptions<BOINCWorkerOptions> options,
    IFileSystem fileSystem,
    ReadOldThrottleSetting readOldThrottleSetting
    )
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger _logger = logger;

    private readonly RunBOINCCmdReadGlobalPrefsOverride ReadGlobalPrefsOverride = new(fileSystem, options.Value.BinaryPath);

    internal readonly FileUpdater globalPreferenceOverrideUpdater = new(fileSystem, options.Value.DataPath, "global_prefs_override.xml");

    private TaskCompletionSource ThrottleHasChanged = new();

    private double newThrottle = 0;

    internal async Task ThrottleCPUPUtilisaion(CancellationToken cancellationToken)
    {
        double throttle = await readOldThrottleSetting.FetchAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            await ApplyCPUThrottle(throttle, cancellationToken);

            LogInformationNewCPUThrottleSetting(throttle);

            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

            await ThrottleHasChanged.Task;
            throttle = newThrottle;
            ThrottleHasChanged = new();
        }
    }

    internal async Task ApplyCPUThrottle(double throttle, CancellationToken cancellationToken = default)
    {
        await globalPreferenceOverrideUpdater.Update((doc) => EditGlobalPrefsOverride(doc, throttle), cancellationToken);

        await ReadGlobalPrefsOverride.Run(cancellationToken);
    }

    private static void EditGlobalPrefsOverride(XDocument globalPreferenceOverride, double throttle)
    {
        var cpu_usage_limit_element = globalPreferenceOverride.XPathSelectElement("/global_preferences/cpu_usage_limit") ?? throw new InvalidOperationException("cpu_usage_limit not present in global_prefs_override.xml");

        cpu_usage_limit_element.Value = throttle.ToString();
    }

    internal async Task UpdateThrottle(double throttle, CancellationToken cancellationToken = default)
    {
        newThrottle = throttle;

        ThrottleHasChanged.TrySetResult();

        await Task.Yield();
    }

    [LoggerMessage(LogLevel.Information, Message = "New CPU throttle setting: {throttle}", EventId = (int)EventIdentifier.NewCPUThrottleSetting)]
    private partial void LogInformationNewCPUThrottleSetting(double throttle);
}
