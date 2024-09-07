// <copyright file="LocalBoincOptions.cs" company="Martin Rudat">
// BOINC To MQTT - Exposes some BOINC controls via MQTT for integration with Home Assistant.
// Copyright (C) 2024  Martin Rudat
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>

namespace BOINC_To_MQTT.Boinc;

using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Net;

/// <summary>
/// Options specific to local BOINC clients.
/// </summary>
public sealed class LocalBoincOptions : CommonBoincOptions
{
    /// <summary>
    /// Gets or sets the path to the BOINC data directory.
    /// </summary>
    [Required]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable. - https://github.com/dotnet/runtime/issues/101984
    public /*required*/ string DataPath { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// Gets or sets the port number for local BOINC client. Only required for more than one local BOINC client.
    /// </summary>
    public ushort? PortNumber { get; set; }

    /// <inheritdoc/>
    internal override string GetHostName() => "localhost";

    /// <inheritdoc/>
    internal override ushort GetPort()
    {
        // FIXME what of multiple local clients?
        return this.PortNumber ?? BoincConnection.BoincRpcPort;
    }

    /// <inheritdoc/>
    internal override async Task<string> GetGuiRpcKeyAsync(IFileSystem fileSystem, CancellationToken cancellationToken) => await fileSystem.File.ReadLinesAsync(fileSystem.Path.Combine(this.DataPath, "gui_rpc_auth.cfg"), cancellationToken).FirstAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    internal override string GetUserReadableDescription()
    {
        if (this.PortNumber is null)
        {
            return $"The BOINC client running from {this.DataPath}";
        }
        else
        {
            return $"The BOINC client running from {this.DataPath} and listening on {this.PortNumber}";
        }
    }

    /// <inheritdoc/>
    internal override string GetName() => $"BOINC on {Dns.GetHostName()}";
}
