// <copyright file="IBoincConnection.cs" company="Martin Rudat">
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

using System.Xml.Linq;
using BoincRpc;

/// <summary>
/// Thin wrapper around <see cref="RpcClient"/>.
/// </summary>
internal interface IBoincConnection
{
    /// <summary>
    /// Returns a <see cref="Device"/> describing this BOINC client.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A <see cref="Device"/> describing this BOINC client.</returns>
    Task<Device> GetDeviceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the contents of the global_prefs_override.xml file, if present.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>The contents of the global_prefs_override.xml.</returns>
    Task<XElement> GetGlobalPreferencesOverrideAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the contents of the currently used global_prefs.xml.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>The contents of the currently used global_prefs.xml.</returns>
    Task<GlobalPreferences> GetGlobalPreferencesWorkingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Client Identifier.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>The Client Identifier.</returns>
    Task<string> GetHostCrossProjectIdentifierAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get information about host hardware and usage.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>The host information.</returns>
    Task<HostInfo> GetHostInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a list of all attached projects.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A list of all projects.</returns>
    Task<Project[]> GetProjectStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs an operation on a project.
    /// </summary>
    /// <param name="project">The project to perform the operation on.</param>
    /// <param name="operation">The operation to perform.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task PerformProjectOperationAsync(Project project, ProjectOperation operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tell the client to reread the global_prefs_override.xml file and set the preferences accordingly.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ReadGlobalPreferencesOverrideAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Write the given contents to the global_prefs_override.xml file or delete it.
    /// </summary>
    /// <param name="globalPreferencesOverride">The contents for the global
    /// preferences override file, or null to delete the file.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetGlobalPreferencesOverrideAsync(XElement? globalPreferencesOverride, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the GPU run mode.
    /// </summary>
    /// <param name="mode">The GPU run mode to set.</param>
    /// <param name="timeSpan">The time span after which the GPU run mode should get reset.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetGpuModeAsync(Mode mode, TimeSpan timeSpan, CancellationToken cancellationToken = default);
}
