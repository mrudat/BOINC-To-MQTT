using System.ComponentModel.DataAnnotations;

namespace BOINCWorker;

public sealed class BOINCWorkerOptions
{
    public const string ConfigurationSectionName = "BOINC";

    /// <summary>
    /// Path to the BOINC data directory.
    /// </summary>
    [Required]
    public required string DataPath;

    /// <summary>
    /// Path to the BOINC binaries.
    /// </summary>
    [Required]
    public required string BinaryPath;
}
