namespace BOINC_To_MQTT;

internal partial class AbstractController : IController
{
    public Task Configure(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task Run(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SetUp(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
