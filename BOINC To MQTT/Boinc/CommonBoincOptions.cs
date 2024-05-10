// Ignore Spelling: BOINC MQTT homeassistant RPC

using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;

namespace BOINC_To_MQTT.Boinc;

public abstract record CommonBoincOptions
{
    /// <summary>
    /// The minimum time to allow the GPU to work for.
    /// </summary>
    [Range(1, int.MaxValue)]
    public uint MinimumGPUWorkTime { get; set; } = 60;

    /// <summary>
    /// The minimum time to allow the GPU to sleep for.
    /// </summary>
    [Range(1, int.MaxValue)]
    public uint MinimumGPUSleepTime { get; set; } = 10;

    /// <summary>
    /// The minimum time to allow the GPU to sleep for.
    /// </summary>
    [Range(1, int.MaxValue)]
    public uint MaximumGPUCycleTime { get; set; } = 300;

    /// <summary>
    /// Gets the host name of the BOINC client.
    /// </summary>
    /// <returns>The host name of the BOINC client.</returns>
    internal abstract string GetHostName();

    /// <summary>
    /// Gets the port number of the BOINC client.
    /// </summary>
    /// <returns>The port number of the BOINC client.</returns>
    internal abstract ushort GetPort();

    /// <summary>
    /// Gets the BOINC GUI RPC key.
    /// </summary>
    /// <returns>The BOINC GUI RPC key.</returns>
    internal abstract Task<string> GetGuiRpcKeyAsync(IFileSystem fileSystem, CancellationToken cancellationToken);
    internal abstract string GetUserReadableDescription();
}
