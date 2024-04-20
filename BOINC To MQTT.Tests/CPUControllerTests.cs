// Ignore Spelling: BOINC

using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using System.Threading;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace BOINC_To_MQTT.Tests;

public class CPUControllerTests(ITestOutputHelper testOutputHelper)
{
    private readonly ILogger<CPUController> logger = XUnitLogger.CreateLogger<CPUController>(testOutputHelper);
    private readonly FakeTimeProvider fakeTimeProvider = new();

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task TestApplyCpuThrottle(double throttle)
    {
        Mock<IBOINCConnection> bOINCConnection = new();

        var cts = new CancellationTokenSource();

        var token = cts.Token;

        XElement doc = new("global_preferences");

        doc.Add(new XElement("cpu_usage_limit", "42"));

        bOINCConnection.Setup(bc => bc.GetGlobalPreferencesOverrideAsync(token)).ReturnsAsync(doc);

        bOINCConnection.Setup(bc => bc.SetGlobalPreferencesOverrideAsync(It.IsAny<XElement>(), token))
            .Callback((XElement? globalPreferencesOverride, CancellationToken _) =>
            {
                globalPreferencesOverride
                    .Should().NotBeNull()
                    .And.HaveElement("cpu_usage_limit")
                    .Which.Should().HaveValue(throttle.ToString());
            });

        bOINCConnection.Setup(bc => bc.ReadGlobalPreferencesOverrideAsync(token)).Callback(cts.Cancel);

        var cpuController = new CPUController(logger, bOINCConnection.Object, fakeTimeProvider);

        cpuController.SetCPUUsageLimit(throttle);

        fakeTimeProvider.AutoAdvanceAmount = TimeSpan.FromSeconds(1);

        var action = async () => await cpuController.Run(token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task TestApplyCpuThrottleAfterWait(double throttle)
    {
        Mock<IBOINCConnection> bOINCConnection = new();

        var cts = new CancellationTokenSource();

        var token = cts.Token;

        XElement doc = new("global_preferences");

        var readCount = 0;

        doc.Add(new XElement("cpu_usage_limit", "42"));

        bOINCConnection.Setup(bc => bc.GetGlobalPreferencesOverrideAsync(token)).ReturnsAsync(doc);

        bOINCConnection.Setup(bc => bc.SetGlobalPreferencesOverrideAsync(It.IsAny<XElement>(), token))
            .Callback((XElement? globalPreferencesOverride, CancellationToken _) =>
            {
                if (readCount == 1)
                {
                    globalPreferencesOverride
                        .Should().NotBeNull()
                        .And.HaveElement("cpu_usage_limit")
                        .Which.Should().HaveValue(throttle.ToString());
                }
            });

        var cpuController = new CPUController(logger, bOINCConnection.Object, fakeTimeProvider);

        bOINCConnection.Setup(bc => bc.ReadGlobalPreferencesOverrideAsync(token))
            .Callback(async (CancellationToken cancellationToken) =>
            {
                readCount++;
                if (readCount >= 2)
                {
                    cts.Cancel();
                    await Task.Yield();
                }
            });

        cpuController.SetCPUUsageLimit(100.0 / 3);

        var action = async () => await cpuController.Run(token);

        Task actionTask = action.Should().ThrowAsync<OperationCanceledException>();

        async Task Foo()
        {
            fakeTimeProvider.Advance(TimeSpan.FromSeconds(15));
            await cpuController.UpdateThrottle(throttle, token);
            await Task.Yield();

            while (!token.IsCancellationRequested)
            {
                fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));
                await Task.Yield();
            }
        }

        await Task.WhenAll([
            actionTask,
            Foo()
        ]);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task TestApplyCpuThrottleMultipleTimes(double throttle)
    {
        Mock<IBOINCConnection> bOINCConnection = new();

        var cts = new CancellationTokenSource();

        var token = cts.Token;

        XElement doc = new("global_preferences");

        var readCount = 0;

        doc.Add(new XElement("cpu_usage_limit", "42"));

        bOINCConnection.Setup(bc => bc.GetGlobalPreferencesOverrideAsync(token)).ReturnsAsync(doc);

        bOINCConnection.Setup(bc => bc.SetGlobalPreferencesOverrideAsync(It.IsAny<XElement>(), token))
            .Callback((XElement? globalPreferencesOverride, CancellationToken _) =>
            {
                if (readCount == 1)
                {
                    globalPreferencesOverride
                        .Should().NotBeNull()
                        .And.HaveElement("cpu_usage_limit")
                        .Which.Should().HaveValue(throttle.ToString());
                }
            });

        var cpuController = new CPUController(logger, bOINCConnection.Object, fakeTimeProvider);

        bOINCConnection.Setup(bc => bc.ReadGlobalPreferencesOverrideAsync(token))
            .Callback(async (CancellationToken cancellationToken) =>
            {
                readCount++;
                if (readCount >= 2)
                {
                    cts.Cancel();
                    await Task.Yield();
                }
            });

        cpuController.SetCPUUsageLimit(100.0 / 3);

        var action = async () => await cpuController.Run(token);

        Task actionTask = action.Should().ThrowAsync<OperationCanceledException>();

        async Task Foo()
        {
            fakeTimeProvider.Advance(TimeSpan.FromSeconds(10));
            await Task.Yield();

            await cpuController.UpdateThrottle(throttle - 1, token);
            fakeTimeProvider.Advance(TimeSpan.FromSeconds(10));
            await Task.Yield();

            await cpuController.UpdateThrottle(throttle + 1, token);
            fakeTimeProvider.Advance(TimeSpan.FromSeconds(5));
            await Task.Yield();

            await cpuController.UpdateThrottle(throttle, token);
            fakeTimeProvider.AutoAdvanceAmount = TimeSpan.FromSeconds(1);
            await Task.Yield();

            while (!token.IsCancellationRequested)
            {
                fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));
                await Task.Yield();
            }
        }

        await Task.WhenAll([
            actionTask,
            Foo()
            ]);
    }

}