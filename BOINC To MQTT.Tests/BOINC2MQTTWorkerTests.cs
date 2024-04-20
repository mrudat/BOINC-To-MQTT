// Ignore Spelling: BOINC Offline

using Microsoft.Extensions.Options;
using Moq;

namespace BOINC_To_MQTT.Tests;

public class BOINC2MQTTWorkerTests
{
    private class Foo(IMQTTConnection mqttClient, IOptions<BOINC2MQTTWorkerOptions> options, IThrottleController throttleController, IWorkController workController) : BOINC2MQTTWorker(mqttClient, options, throttleController, workController)
    {
        internal Task InternalExecuteAsync(CancellationToken cancellationToken = default) => ExecuteAsync(cancellationToken);
    }

    [Theory]
    [InlineData("topic")]
    [InlineData("prefix")]
    public async Task TestExecuteAsync(string discoveryPrefix)
    {
        Mock<IMQTTConnection> mockMqttClient = new(MockBehavior.Strict);
        Mock<IOptions<BOINC2MQTTWorkerOptions>> mockOptions = new(MockBehavior.Strict);
        Mock<IThrottleController> mockThrottleController = new(MockBehavior.Strict);
        Mock<IWorkController> mockWorkController = new(MockBehavior.Strict);

        var cts = new CancellationTokenSource();

        var token = cts.Token;

        var subscriptionTopic = $"{discoveryPrefix}/status";

        mockMqttClient.Setup(mqttClient => mqttClient.RegisterSubscription(subscriptionTopic, It.IsAny<SubscriptionCallback>(), token))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        mockOptions.Setup(options => options.Value)
            .Returns(new BOINC2MQTTWorkerOptions()
            {
                BOINC = new()
                {
                    BinaryPath = "",
                    DataPath = ""
                },
                MQTT = new()
                {
                    DiscoveryPrefix = discoveryPrefix,
                }
            });

        mockThrottleController.Setup(throttleController => throttleController.SetUp(token))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        mockThrottleController.Setup(throttleController => throttleController.Run(token))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        mockWorkController.Setup(workController => workController.SetUp(token))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        mockWorkController.Setup(workController => workController.Run(token))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        var w = new Foo(
            mockMqttClient.Object,
            mockOptions.Object,
            mockThrottleController.Object,
            mockWorkController.Object);

        await w.InternalExecuteAsync(token);

        Mock.Verify([
            mockMqttClient,
            mockOptions,
            mockThrottleController,
            mockWorkController
        ]);
    }

    [Fact]
    public async Task TestConfigureCallbackOnline()
    {
        Mock<IMQTTConnection> mockMqttClient = new(MockBehavior.Strict);
        Mock<IOptions<BOINC2MQTTWorkerOptions>> mockOptions = new(MockBehavior.Strict);
        Mock<IThrottleController> mockThrottleController = new(MockBehavior.Strict);
        Mock<IWorkController> mockWorkController = new(MockBehavior.Strict);

        var cancellationTokenSource = new CancellationTokenSource();

        var token = cancellationTokenSource.Token;

        mockThrottleController.Setup(throttleController => throttleController.Configure(token))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        mockWorkController.Setup(workController => workController.Configure(token))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        var w = new BOINC2MQTTWorker(
            mockMqttClient.Object,
            mockOptions.Object,
            mockThrottleController.Object,
            mockWorkController.Object);

        await w.ConfigureCallback("topic", "online", token);

        Mock.Verify([
            mockMqttClient,
            mockOptions,
            mockThrottleController,
            mockWorkController
        ]);
    }

    [Fact]
    public async Task TestConfigureCallbackOffline()
    {
        Mock<IMQTTConnection> mockMqttClient = new(MockBehavior.Strict);
        Mock<IOptions<BOINC2MQTTWorkerOptions>> mockOptions = new(MockBehavior.Strict);
        Mock<IThrottleController> mockThrottleController = new(MockBehavior.Strict);
        Mock<IWorkController> mockWorkController = new(MockBehavior.Strict);

        var cancellationTokenSource = new CancellationTokenSource();

        var token = cancellationTokenSource.Token;

        var w = new BOINC2MQTTWorker(
            mockMqttClient.Object,
            mockOptions.Object,
            mockThrottleController.Object,
            mockWorkController.Object);

        await w.ConfigureCallback("topic", "offline", token);

        Mock.Verify([
            mockMqttClient,
            mockOptions,
            mockThrottleController,
            mockWorkController
        ]);
    }
}
