// Ignore Spelling: MQTT username TLS EMQX

using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using System.Text.RegularExpressions;

namespace Testcontainers.EMQX;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class EmqxBuilder : ContainerBuilder<EmqxBuilder, EmqxContainer, EmqxConfiguration>
{
    public const string EmqxImage = "emqx/emqx:5.6.0";

    public const string DefaultUsername = "username";

    public const ushort MqttPort = 1883;

    public const ushort MqttTlsPort = 8883;

    public const ushort MqttWebSocketsPort = 8083;

    public const ushort MqttWebSocketsTlsPort = 8084;

    public const ushort DashboardPort = 18083;

    public EmqxBuilder() : this(new EmqxConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    private EmqxBuilder(EmqxConfiguration resourceConfiguration) : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override EmqxConfiguration DockerResourceConfiguration { get; }


    /// <inheritdoc />
    public override EmqxContainer Build()
    {
        Validate();
        return new EmqxContainer(DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override EmqxBuilder Init()
    {
        return base.Init()
        .WithImage(EmqxImage)
        .WithPortBinding(MqttPort, true)
        .WithPortBinding(MqttTlsPort, true)
        .WithPortBinding(MqttWebSocketsPort, true)
        .WithPortBinding(MqttWebSocketsTlsPort, true)
        .WithPortBinding(DashboardPort, true)
#if NET7_0_OR_GREATER
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(EMQXIsRunning()));
#else
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(new Regex(@"EMQX \S+ is running now!")));
#endif
    }

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"EMQX \S+ is running now!")]
    private static partial Regex EMQXIsRunning();
#endif

    /// <inheritdoc />
    //protected override void Validate() => base.Validate();

    /// <inheritdoc />
    protected override EmqxBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new EmqxConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override EmqxBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new EmqxConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override EmqxBuilder Merge(EmqxConfiguration oldValue, EmqxConfiguration newValue)
    {
        return new EmqxBuilder(new EmqxConfiguration(oldValue, newValue));
    }
}
