// Ignore Spelling: BOINC Dockerfile mosquitto openssl emqx username passwd mqtt websockets certfile keyfile
using Docker.DotNet.Models;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using System.Text;
using System.Text.RegularExpressions;

namespace Testcontainers.Mosquitto;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class MosquittoBuilder : ContainerBuilder<MosquittoBuilder, MosquittoContainer, MosquittoConfiguration>
{
    public const string MosquittoImage = "eclipse-mosquitto:2.0.18-openssl";

    public const string DefaultUsername = "username";

    public const string DefaultPassword = "password";

    public const ushort MqttPort = 1883;

    public const ushort MqttWebSocketsPort = 8080;

    private static readonly byte[] MosquittoConfFile = Encoding.UTF8.GetBytes($"""
        per_listener_settings true

        listener {MqttPort}
        protocol mqtt
        password_file /mosquitto/config/passwd

        listener {MqttWebSocketsPort}
        protocol websockets
        password_file /mosquitto/config/passwd
        """);

    public MosquittoBuilder() : this(new MosquittoConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    private MosquittoBuilder(MosquittoConfiguration resourceConfiguration) : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override MosquittoConfiguration DockerResourceConfiguration { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="username"></param>
    /// <returns>A configured instance of <see cref="MosquittoBuilder"/></returns>
    public MosquittoBuilder WithUsername(string username)
    {
        return Merge(DockerResourceConfiguration, new MosquittoConfiguration(username: username));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="password"></param>
    /// <returns>A configured instance of <see cref="MosquittoBuilder"/></returns>
    public MosquittoBuilder WithPassword(string password)
    {
        return Merge(DockerResourceConfiguration, new MosquittoConfiguration(password: password));
    }

    /// <inheritdoc />
    public override MosquittoContainer Build()
    {
        Validate();

        return new MosquittoContainer(DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override MosquittoBuilder Init()
    {
        return base.Init()
            .WithImage(MosquittoImage)
            .WithPortBinding(MqttPort, true)
            .WithPortBinding(MqttWebSocketsPort, true)
            .WithResourceMapping(Array.Empty<byte>(), "/mosquitto/config/passwd", UnixFileModes.UserRead | UnixFileModes.UserWrite)
            .WithResourceMapping(MosquittoConfFile, "/mosquitto/config/mosquitto.conf", UnixFileModes.UserRead | UnixFileModes.UserWrite)
            .WithUsername(DefaultUsername)
            .WithPassword(DefaultPassword)
            .WithStartupCallback(StartupCallback)
#if NET7_0_OR_GREATER
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(MosquittoIsRunning()));
#else
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(new Regex(@"mosquitto version \S+ running")));
#endif
    }

    private async Task StartupCallback(MosquittoContainer container, CancellationToken token)
    {
        await (container as IRequiresAuthentication).AddUser(DockerResourceConfiguration.Username!, DockerResourceConfiguration.Password!, token);
    }

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"mosquitto version \S+ running")]
    private static partial Regex MosquittoIsRunning();
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
    protected override MosquittoBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new MosquittoConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override MosquittoBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new MosquittoConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override MosquittoBuilder Merge(MosquittoConfiguration oldValue, MosquittoConfiguration newValue)
    {
        return new MosquittoBuilder(new MosquittoConfiguration(oldValue, newValue));
    }
}
