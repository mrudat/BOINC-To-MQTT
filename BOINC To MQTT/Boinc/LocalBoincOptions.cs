// Ignore Spelling: BOINC MQTT homeassistant RPC

using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;

namespace BOINC_To_MQTT.Boinc;

public sealed record LocalBoincOptions : CommonBoincOptions
{
    /// <summary>
    /// Path to the BOINC data directory.
    /// </summary>
    [Required]
#pragma warning disable CS8618 // Workaround for https://github.com/dotnet/runtime/issues/101984.
    public /*required*/ string DataPath { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// Port number for local BOINC client. Only required for more than one local BOINC client.
    /// </summary>
    public ushort? PortNumber { get; set; }

    internal override string GetHostName() => "localhost";

    // FIXME what of multiple local clients?
    internal override ushort GetPort() => PortNumber ?? BoincConnection.BoincRpcPort;

    internal override async Task<string> GetGuiRpcKeyAsync(IFileSystem fileSystem, CancellationToken cancellationToken)
    {
        string guiRpcKeyFile = fileSystem.Path.Combine(DataPath, "gui_rpc_auth.cfg");

        return await fileSystem.File.ReadLinesAsync(guiRpcKeyFile, cancellationToken).FirstAsync(cancellationToken);
    }

    internal override string GetUserReadableDescription()
    {
        if (PortNumber is null)
        {
            return $"The BOINC client running from {DataPath}";
        }
        else
        {
            return $"The BOINC client running from {DataPath} and listening on {PortNumber}";
        }
    }
}
