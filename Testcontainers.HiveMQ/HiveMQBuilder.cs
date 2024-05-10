// Ignore Spelling: MQTT username hivemq

using Docker.DotNet.Models;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using System.Text.RegularExpressions;

namespace Testcontainers.HiveMQ;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class HiveMQBuilder : ContainerBuilder<HiveMQBuilder, HiveMQContainer, HiveMQConfiguration>
{
    public const string HiveMQImage = "hivemq/hivemq4:4.28.0";

    public const string DefaultUsername = "username";

    public const string DefaultPassword = "password";

    public const ushort MqttPort = 1883;

    public const ushort WebSocketsPort = 8000;

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="username"></param>
    /// <returns>A configured instance of <see cref="HiveMQBuilder"/></returns>
    public HiveMQBuilder WithUsername(string username)
    {
        return Merge(DockerResourceConfiguration, new HiveMQConfiguration(username: username));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="password"></param>
    /// <returns>A configured instance of <see cref="HiveMQBuilder"/></returns>
    public HiveMQBuilder WithPassword(string password)
    {
        return Merge(DockerResourceConfiguration, new HiveMQConfiguration(password: password));
    }

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
        .WithUsername(DefaultUsername)
        .WithPassword(DefaultPassword)
        .WithPortBinding(MqttPort, true)
        .WithPortBinding(WebSocketsPort, true)
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
    protected override void Validate()
    {
        base.Validate();

        _ = Guard.Argument(DockerResourceConfiguration.Username, nameof(DockerResourceConfiguration.Username))
            .NotNull()
            .NotEmpty();

        _ = Guard.Argument(DockerResourceConfiguration.Password, nameof(DockerResourceConfiguration.Password))
            .NotNull()
            .NotEmpty();
    }

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
