using FluentAssertions;

namespace BOINC_To_MQTT.Tests;

public class AsyncTaskCompletionSourceTests
{
    [Fact]
    public async Task Test1()
    {
        var atcs = new AsyncTaskCompletionSource<int>();
        await atcs.SetResult(1);
        var result = await atcs.Task;
        result.Should().Be(1);
    }

    [Fact]
    public async Task Test2()
    {
        var atcs = new AsyncTaskCompletionSource();
        await atcs.SetResult();
        await atcs.Task;
    }
}
