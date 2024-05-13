// Ignore Spelling: MQTT username hivemq

using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using System.Text.RegularExpressions;

namespace Testcontainers.HiveMQ;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class HiveMQBuilder : ContainerBuilder<HiveMQBuilder, HiveMQContainer, HiveMQConfiguration>
{
    public const string HiveMQImage = "hivemq/hivemq4:4.28.0";

    public const string DefaultUsername = "username";

    public const ushort MqttPort = 1883;

    public const ushort MqttWebSocketsPort = 8000;

    public HiveMQBuilder() : this(new HiveMQConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    private HiveMQBuilder(HiveMQConfiguration resourceConfiguration) : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override HiveMQConfiguration DockerResourceConfiguration { get; }

    /// <inheritdoc />
    public override HiveMQContainer Build()
    {
        Validate();

        var finalConfig = this;

        return new HiveMQContainer(finalConfig.DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override HiveMQBuilder Init()
    {
        return base.Init()
        .WithImage(HiveMQImage)
        .WithPortBinding(MqttPort, true)
        .WithPortBinding(MqttWebSocketsPort, true)
#if NET7_0_OR_GREATER
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(HiveMQIsRunning()));
#else
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(new Regex(@"Started HiveMQ")));
#endif
    }

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"Started HiveMQ")]
    private static partial Regex HiveMQIsRunning();
#endif

    /// <inheritdoc />
    //protected override void Validate() => base.Validate();

    /// <inheritdoc />
    protected override HiveMQBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new HiveMQConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override HiveMQBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new HiveMQConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override HiveMQBuilder Merge(HiveMQConfiguration oldValue, HiveMQConfiguration newValue)
    {
        return new HiveMQBuilder(new HiveMQConfiguration(oldValue, newValue));
    }
}
