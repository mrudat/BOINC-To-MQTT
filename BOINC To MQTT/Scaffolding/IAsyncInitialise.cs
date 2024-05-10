// Ignore Spelling: offline MQTT unregister

namespace BOINC_To_MQTT.Scaffolding;

internal interface IAsyncInitialise
{
    internal Task Initialise(CancellationToken cancellationToken);
}

internal interface IAsyncInitialise<T>
{
    internal Task<T> Initialise(CancellationToken cancellationToken);
}
