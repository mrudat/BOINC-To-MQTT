namespace BOINC_To_MQTT.Scaffolding;

internal interface IRequiresConfiguration
{
    internal Task ConfigureAsync(CancellationToken cancellationToken = default);
}