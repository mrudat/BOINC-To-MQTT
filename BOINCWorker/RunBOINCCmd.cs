using System.Diagnostics;
using System.IO.Abstractions;

namespace BOINCWorker;

internal abstract class RunBOINCCmd
{
    protected readonly ProcessStartInfo BOINCCmdProcessStartInfo;

    public RunBOINCCmd(IFileSystem fileSystem, string BinaryPath)
    {
        BOINCCmdProcessStartInfo = AddArguments(new()
        {
            UseShellExecute = false,
            FileName = fileSystem.Path.Combine(BinaryPath, "boinccmd"),
            CreateNoWindow = true,
        });
    }

    protected abstract ProcessStartInfo AddArguments(ProcessStartInfo processStartInfo);

}