// Ignore Spelling: BOINC MQTT
using TestContainers.BOINC;

namespace BOINC_To_MQTT.Tests;

public class BoincFixture : IAsyncLifetime
{
    public readonly BoincContainer Container = new BoincBuilder()
            // TODO capture container output.
            //.WithLogger(logger)
            .Build();

    Task IAsyncLifetime.InitializeAsync() => Container.StartAsync();

    Task IAsyncLifetime.DisposeAsync() => Container.DisposeAsync().AsTask();
}
