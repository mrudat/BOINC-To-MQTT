using System.Diagnostics;
using System.IO.Abstractions;

namespace BOINCWorker;

internal class RunBOINCCmdReadGlobalPrefsOverride(IFileSystem fileSystem, string BinaryPath) : RunBOINCCmd(fileSystem, BinaryPath)
{
    protected override ProcessStartInfo AddArguments(ProcessStartInfo processStartInfo)
    {
        var argumentList = processStartInfo.ArgumentList;
        argumentList[0] = "--read_global_prefs_override";

        return processStartInfo;
    }

    internal async Task Run(CancellationToken cancellationToken = default)
    {
        await BOINCWorkerHelpers.RunProcessAsync(BOINCCmdProcessStartInfo, cancellationToken);
    }
}