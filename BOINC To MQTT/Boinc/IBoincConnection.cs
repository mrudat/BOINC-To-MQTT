using BoincRpc;
using System.Xml.Linq;

namespace BOINC_To_MQTT.Boinc;

/// <summary>
/// Thin wrapper around <see cref="RpcClient"/>.
/// </summary>
internal interface IBoincConnection
{
    /// <summary>
    /// Gets the Client Identifier
    /// </summary>
    /// <returns>The Client Identifier</returns>
    Task<string> GetHostCrossProjectIdentifierAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the contents of the global_prefs_override.xml file, if present.
    /// </summary>
    /// <returns>The contents of the global_prefs_override.xml.</returns>
    Task<XElement> GetGlobalPreferencesOverrideAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the contents of the currently used global_prefs.xml.
    /// </summary>
    /// <returns>The contents of the currently used global_prefs.xml.</returns>
    Task<GlobalPreferences> GetGlobalPreferencesWorkingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get information about host hardware and usage.
    /// </summary>
    /// <returns>The host information.</returns>
    Task<HostInfo> GetHostInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a list of all attached projects.
    /// </summary>
    /// <returns>A list of all projects.</returns>
    Task<Project[]> GetProjectStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs an operation on a project.
    /// </summary>
    /// <param name="project">The project to perform the operation on.</param>
    /// <param name="operation">The operation to perform.</param>
    Task PerformProjectOperationAsync(Project project, ProjectOperation operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tell the client to reread the global_prefs_override.xml file and set the preferences accordingly
    /// </summary>
    Task ReadGlobalPreferencesOverrideAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Write the given contents to the global_prefs_override.xml file or delete it.
    /// </summary>
    /// <param name="globalPreferencesOverride">The contents for the global
    /// preferences override file, or null to delete the file.</param>
    Task SetGlobalPreferencesOverrideAsync(XElement? globalPreferencesOverride, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the GPU run mode.
    /// </summary>
    /// <param name="mode">The GPU run mode to set.</param>
    /// <param name="timeSpan">The time span after which the GPU run mode should get reset.</param>
    Task SetGpuModeAsync(Mode mode, TimeSpan timeSpan, CancellationToken cancellationToken = default);
}