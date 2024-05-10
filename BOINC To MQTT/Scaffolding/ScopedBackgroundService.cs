namespace BOINC_To_MQTT.Scaffolding;

/// <summary>
/// A version of <see cref="IHostedService"/> to identify scoped background services.
/// </summary>
internal abstract class ScopedBackgroundService : BackgroundService, IScopedHostedService
{

}