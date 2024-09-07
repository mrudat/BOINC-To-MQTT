// <copyright file="BoincContainer.cs" company="Martin Rudat">
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

namespace Testcontainers.BOINC;

using System.Text;
using DotNet.Testcontainers.Containers;

/// <summary>
/// Initializes a new instance of the <see cref="BoincContainer"/> class.
/// </summary>
/// <param name="configuration">The configuration to use.</param>
public class BoincContainer(BoincConfiguration configuration) : DockerContainer(configuration)
{
    /// <summary>
    /// Gets the BOINC GUI's RPC port.
    /// </summary>
    public ushort GuiRpcPort => this.GetMappedPublicPort(BoincBuilder.GuiRpcPort);

    /// <summary>
    /// Gets the BOINC GUI's RPC key.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>The BOINC GUI's RPC key.</returns>
    public async Task<string> GetGuiRpcKeyAsync(CancellationToken cancellationToken = default)
    {
        var contents = await this.ReadFileAsync("/var/lib/boinc/gui_rpc_auth.cfg", cancellationToken).ConfigureAwait(false);
        using var ms = new MemoryStream(contents);
        using var sr = new StreamReader(ms, encoding: Encoding.ASCII);
        return sr.ReadLine()!;
    }
}
