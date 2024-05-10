// Ignore Spelling: BOINC MQTT homeassistant RPC

using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;

namespace BOINC_To_MQTT.Boinc;

public sealed record RemoteBoincOptions : CommonBoincOptions
{
    /// <summary>
    /// URI for a remote BOINC instance
    /// </summary>
    [Required]
#pragma warning disable CS8618 // Workaround for https://github.com/dotnet/runtime/issues/101984.
    public /*required*/ Uri BoincUri { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// GUI RPC Key for a remote BOINC instance
    /// </summary>
    [Required]
#pragma warning disable CS8618 // Workaround for https://github.com/dotnet/runtime/issues/101984.
    public /*required*/ string GuiRpcKey { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    internal override Task<string> GetGuiRpcKeyAsync(IFileSystem _1, CancellationToken _2) => Task.FromResult(GuiRpcKey);

    internal override string GetHostName() => BoincUri.Host;

    internal override ushort GetPort() => BoincUri.IsDefaultPort ? BoincConnection.BoincRpcPort : (ushort)BoincUri.Port;

    internal override string GetUserReadableDescription() => $"the BOINC client listening on {BoincUri}";
}
