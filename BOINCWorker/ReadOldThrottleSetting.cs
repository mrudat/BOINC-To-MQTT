using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BOINCWorker;

internal partial class ReadOldThrottleSetting(
    ILogger<ReadOldThrottleSetting> logger,
    IOptions<BOINCWorkerOptions> options,
    IFileSystem fileSystem
)
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger _logger = logger;

    private readonly IFileStreamFactory FileStream = fileSystem.FileStream;

    readonly string filePath = fileSystem.Path.Combine(options.Value.DataPath, "global_prefs_override.xml");

    Task<double>? theTask;

    internal async Task<double> FetchAsync(CancellationToken cancellationToken = default)
    {
        if (theTask is null)
        {
            theTask = FetchAsyncInternal(cancellationToken);
            return await theTask;
        }
        else
        {
            return await theTask.WaitAsync(cancellationToken);
        }
    }

    internal async Task<double> FetchAsyncInternal(CancellationToken cancellationToken = default)
    {
        using var readFileHandle = await Task.Run(() => FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), cancellationToken);

        var document = await XDocument.LoadAsync(readFileHandle, LoadOptions.None, cancellationToken);

        var cpuUsageLimitElement = document.XPathSelectElement("/global_preferences/cpu_usage_limit") ?? throw new InvalidOperationException("cpu_usage_limit not present in global_prefs_override.xml");

        var cpuUsageLimit = double.Parse(cpuUsageLimitElement.Value);

        LogInformationReadOldCPULimit(cpuUsageLimit);

        return cpuUsageLimit;
    }

    [LoggerMessage(EventId = (int)EventIdentifier.ReadCPUUsageLimit, Level = LogLevel.Information, Message = "Read CPU usage limit {cpuUsageLimit} from global_prefs_override.xml")]
    private partial void LogInformationReadOldCPULimit(double cpuUsageLimit);
}