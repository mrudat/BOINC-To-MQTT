// <copyright file="RemoteBoincOptions.cs" company="Martin Rudat">
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

/// <summary>
/// Options specific to remote BOINC clients.
/// </summary>
public sealed class RemoteBoincOptions : CommonBoincOptions
{
    /// <summary>
    /// Gets or sets the <see cref="Uri"/> for a remote BOINC instance.
    /// </summary>
    [Required]
#pragma warning disable CS8618 // Workaround for https://github.com/dotnet/runtime/issues/101984.
    public /*required*/ Uri BoincUri { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// Gets or sets the GUI RPC Key for a remote BOINC instance.
    /// </summary>
    [Required]
#pragma warning disable CS8618 // Workaround for https://github.com/dotnet/runtime/issues/101984.
    public /*required*/ string GuiRpcKey { get; set; }

    /// <inheritdoc/>
    internal override Task<string> GetGuiRpcKeyAsync(IFileSystem fileSystem, CancellationToken cancellationToken) => Task.FromResult(this.GuiRpcKey);

    /// <inheritdoc/>
    internal override string GetHostName() => this.BoincUri.Host;

    /// <inheritdoc/>
    internal override string GetName() => $"BOINC on {this.BoincUri.Host}";

    /// <inheritdoc/>
    internal override ushort GetPort() => this.BoincUri.IsDefaultPort ? BoincConnection.BoincRpcPort : (ushort)this.BoincUri.Port;

    /// <inheritdoc/>
    internal override string GetUserReadableDescription() => $"the BOINC client listening on {this.BoincUri}";
}
