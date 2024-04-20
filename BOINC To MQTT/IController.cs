namespace BOINC_To_MQTT;

internal interface IController
{
    public Task Run(CancellationToken cancellationToken = default);

    public Task SetUp(CancellationToken cancellationToken = default);

    public Task Configure(CancellationToken cancellationToken = default);
}
