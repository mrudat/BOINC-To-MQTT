// Ignore Spelling: MQTT Testcontainers Initialize TLS initialization URI mqtts wss

using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Xunit.Abstractions;

namespace Testcontainers.Tests;

public abstract partial class AbstractMqttContainerTests<TContainer, TBuilder, TInterface>(ITestOutputHelper testOutputHelper, ContainerFixture<TContainer, TBuilder> containerFixture)
    where TContainer : class, ICommonMqttContainer, TInterface
    where TBuilder : class, IContainerBuilder<TBuilder, TContainer>, new()
{
    protected readonly ILogger<AbstractMqttContainerTests<TContainer, TBuilder, TInterface>> logger = XUnitLogger.CreateLogger<AbstractMqttContainerTests<TContainer, TBuilder, TInterface>>(testOutputHelper);

    protected readonly MqttFactory mqttFactory = new();

    protected TContainer Container => containerFixture.Container;

    protected TContainer ContainerOnNetwork => containerFixture.ContainerOnNetwork;

    protected async Task AbstractTestCanConnectAsync(TInterface container)
    {
        Uri uri = GetUri(container);

        using var mqttClient = await ConnectTo(uri);

        mqttClient.IsConnected.Should().BeTrue();
    }

    protected void AbstractTestGetNetworkUriFails(TInterface container)
    {
        Action action = () =>
        {
            Uri uri = GetNetworkUri(container);
        };

        action.Should().Throw<InvalidOperationException>();
    }

    protected void AbstractTestGetNetworkUri(TInterface containerOnNetwork)
    {
        Uri uri = GetNetworkUri(containerOnNetwork);

        // TODO check that a mqtt client can actually connect to uri?
    }

    protected abstract Uri GetUri(TInterface container, string? name = null);

    protected abstract Uri GetNetworkUri(TInterface container, string? name = null);

    protected async Task<IMqttClient> ConnectTo(Uri uri)
    {
        var mqttClient = mqttFactory.CreateMqttClient();

        var mqttClientOptionsBuilder = mqttFactory.CreateClientOptionsBuilder();

#pragma warning disable CS0618 // Type or member is obsolete - No valid replacement for WithConnectionUri
        mqttClientOptionsBuilder
            .WithConnectionUri(uri);
#pragma warning restore CS0618 // Type or member is obsolete

        var usingTls = uri.Scheme.ToLowerInvariant() switch
        {
            "mqtts" or "wss" => true,
            _ => false,
        };

        if (usingTls)
        {
            // TODO fetch certificate to compare
            //var cert = await (container as IMqttTlsContainer)?.GetServerCertificateAsync();

            mqttClientOptionsBuilder
                .WithTlsOptions((opt) =>
                {
                    opt.WithCertificateValidationHandler((args) =>
                    {
                        //return args.Certificate.Equals(cert);
                        return true;
                    });
                });
        }

        var mqttClientOptions = mqttClientOptionsBuilder
            .Build();

        LogConnecting(uri);

        await mqttClient.ConnectAsync(mqttClientOptions);

        return mqttClient;
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Connecting to {uri}")]
    private partial void LogConnecting(Uri uri);
}
