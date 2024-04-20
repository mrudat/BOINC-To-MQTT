// Ignore Spelling: BOINC

using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit.Abstractions;

namespace BOINC_To_MQTT.Tests;

public class GPUControllerTests(ITestOutputHelper testOutputHelper)
{
    private readonly ILogger<GPUController> logger = XUnitLogger.CreateLogger<GPUController>(testOutputHelper);
    private readonly FakeTimeProvider fakeTimeProvider = new();


    [Fact]
    public async Task Test1()
    {
        var bOINCClient = new Mock<IBOINCConnection>();

        var gpuController = new GPUController(logger, bOINCClient.Object, fakeTimeProvider);
    }
}