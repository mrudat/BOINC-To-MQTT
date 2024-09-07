// <copyright file="CommonBoincOptions.cs" company="Martin Rudat">
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

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

/// <summary>
/// Options shared between local and remote BOINC clients.
/// </summary>
[SuppressMessage("Major Code Smell", "S4035:Classes implementing \"IEquatable<T>\" should be sealed", Justification = "The Equals implementation sets a requirement for all valid subclasses.")]
public abstract class CommonBoincOptions : IEquatable<CommonBoincOptions?>
{
    /// <summary>
    /// Gets or sets the minimum time to allow the GPU to sleep for.
    /// </summary>
    [Range(1, int.MaxValue)]
    public uint MaximumGPUCycleTime { get; set; } = 300;

    /// <summary>
    /// Gets or sets the minimum time to allow the GPU to sleep for.
    /// </summary>
    [Range(1, int.MaxValue)]
    public uint MinimumGPUSleepTime { get; set; } = 10;

    /// <summary>
    /// Gets or sets the minimum time to allow the GPU to work for.
    /// </summary>
    [Range(1, int.MaxValue)]
    public uint MinimumGPUWorkTime { get; set; } = 60;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as CommonBoincOptions);

    /// <inheritdoc/>
    public bool Equals(CommonBoincOptions? other) => other is not null &&
        this.GetHostName() == other.GetHostName() &&
        this.GetPort() == other.GetPort();

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(this.GetHostName(), this.GetPort());

    /// <summary>
    /// Gets the BOINC GUI RPC key.
    /// </summary>
    /// <param name="fileSystem">A <see cref="IFileSystem"/> to use.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>The BOINC GUI RPC key.</returns>
    internal abstract Task<string> GetGuiRpcKeyAsync(IFileSystem fileSystem, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the host name of the BOINC client.
    /// </summary>
    /// <returns>The host name of the BOINC client.</returns>
    internal abstract string GetHostName();

    /// <inheritdoc cref="IBoincContext.GetName" />
    internal abstract string GetName();

    /// <summary>
    /// Gets the port number of the BOINC client.
    /// </summary>
    /// <returns>The port number of the BOINC client.</returns>
    internal abstract ushort GetPort();

    /// <inheritdoc cref="IBoincContext.GetUserReadableDescription" />
    internal abstract string GetUserReadableDescription();
}
