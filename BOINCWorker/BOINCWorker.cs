using CommonStuff;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using System.IO.Abstractions;

namespace BOINCWorker;

internal partial class BOINCWorker(
    ILogger<BOINCWorker> logger,
    IOptions<BOINCWorkerOptions> options,
    IThingy mQTTWorker,
    IFileSystem fileSystem,
    CPUController cPUController,
    GPUController gPUController
    ) : BackgroundService
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger _logger = logger;

    // TODO lazy instead of nullable?
    private double? throttle = null;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // TODO subscribe to control topic
        {
            MqttClientSubscribeOptionsBuilder sb = new();
            sb.WithTopicFilter("...");

            MqttClientSubscribeOptions subscribeOptions = sb.Build();

            await mQTTWorker.RegisterSubscription(subscribeOptions, ThrottleCallback);
        }

        // TODO publish configuration data
        {
            MqttApplicationMessageBuilder mb = new();
            mb.WithRetainFlag();
            MqttApplicationMessage configurationMessage = mb.Build();
            await mQTTWorker.PublishMessage(configurationMessage, cancellationToken);
        }

        // TODO read old throttle setting from disk.

        // Do the following forever:

        {
            int waitTime = 1000;
            while (!throttle.HasValue)
            {
                await Task.Delay(waitTime, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                waitTime *= 2;
            }
        }

        await Task.WhenAll([
            gPUController.ThrottleGUPUtilisaion(cancellationToken),
            cPUController.ThrottleCPUPUtilisaion(cancellationToken)
        ]);
    }

    private async Task ThrottleCallback(MqttApplicationMessageReceivedEventArgs args, CancellationToken cancellationToken)
    {
        if (!double.TryParse(args.ApplicationMessage.ConvertPayloadToString(), out var bar))
            return;

        throttle = bar switch
        {
            > 100 => 100,
            < 10 => 10,
            _ => bar,
        };

        await Task.WhenAll([
            cPUController.UpdateThrottle(throttle.Value, cancellationToken),
            gPUController.UpdateThrottle(throttle.Value, cancellationToken)
        ]);
    }
}
