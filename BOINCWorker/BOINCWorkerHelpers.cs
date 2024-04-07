using System.Diagnostics;

internal static class BOINCWorkerHelpers
{
    internal static async Task RunProcessAsync(ProcessStartInfo pauseGPUWorkProcessInfo, CancellationToken cancellationToken)
    {
        using Process pauseGPUWorkProcess = new() { StartInfo = pauseGPUWorkProcessInfo };

        pauseGPUWorkProcess.Start();

        await pauseGPUWorkProcess.WaitForExitAsync(cancellationToken);
    }
}