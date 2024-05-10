// Ignore Spelling: MQTT username TLS

using Docker.DotNet.Models;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using System.Text.RegularExpressions;

namespace Testcontainers.EMQX;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class EmqxBuilder : ContainerBuilder<EmqxBuilder, EmqxContainer, EMQXConfiguration>
{
    public const string EMQXImage = "emqx/emqx:5.6.0";

    public const string DefaultUsername = "username";

    public const string DefaultPassword = "password";

    public const ushort MqttPort = 1883;

    public const ushort MqttTlsPort = 8883;

    public const ushort WebSocketsPort = 8083;

    public const ushort WebSocketsTlsPort = 8084;

    public const ushort DashboardPort = 18083;

    public EmqxBuilder() : this(new EMQXConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    private EmqxBuilder(EMQXConfiguration resourceConfiguration) : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override EMQXConfiguration DockerResourceConfiguration { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="username"></param>
    /// <returns>A configured instance of <see cref="EmqxBuilder"/></returns>
    public EmqxBuilder WithUsername(string username)
    {
        return Merge(DockerResourceConfiguration, new EMQXConfiguration(username: username));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="password"></param>
    /// <returns>A configured instance of <see cref="EmqxBuilder"/></returns>
    public EmqxBuilder WithPassword(string password)
    {
        return Merge(DockerResourceConfiguration, new EMQXConfiguration(password: password));
    }

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
        .WithImage(EMQXImage)
        .WithUsername(DefaultUsername)
        .WithPassword(DefaultPassword)
        .WithPortBinding(MqttPort, true)
        .WithPortBinding(MqttTlsPort, true)
        .WithPortBinding(WebSocketsPort, true)
        .WithPortBinding(WebSocketsTlsPort, true)
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
    protected override EmqxBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new EMQXConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override EmqxBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new EMQXConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override EmqxBuilder Merge(EMQXConfiguration oldValue, EMQXConfiguration newValue)
    {
        return new EmqxBuilder(new EMQXConfiguration(oldValue, newValue));
    }
}
