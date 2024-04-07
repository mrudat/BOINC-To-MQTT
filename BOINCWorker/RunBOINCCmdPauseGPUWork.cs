using System.Diagnostics;
using System.IO.Abstractions;

namespace BOINCWorker;

internal class RunBOINCCmdPauseGPUWork(IFileSystem fileSystem, string BinaryPath) : RunBOINCCmd(fileSystem, BinaryPath)
{
    protected override ProcessStartInfo AddArguments(ProcessStartInfo processStartInfo)
    {
        var argumentList = processStartInfo.ArgumentList;
        argumentList[0] = "--set_gpu_mode";
        argumentList[1] = "never";

        return processStartInfo;
    }

    internal async Task Run(int offTime, CancellationToken cancellationToken = default)
    {
        BOINCCmdProcessStartInfo.ArgumentList[2] = offTime.ToString();

        await BOINCWorkerHelpers.RunProcessAsync(BOINCCmdProcessStartInfo, cancellationToken);
    }
}
