// Ignore Spelling: RPC

using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace Testcontainers.HomeAssistant;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class HomeAssistantBuilder : ContainerBuilder<HomeAssistantBuilder, HomeAssistantContainer, HomeAssistantConfiguration>
{
    public const string HomeAssistantImage = "ghcr.io/home-assistant/home-assistant:2022.2";

    public const ushort WebUIPort = 8123;
    public const ushort RpcPort = 40000;

    public HomeAssistantBuilder() : this(new HomeAssistantConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    private HomeAssistantBuilder(HomeAssistantConfiguration resourceConfiguration) : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override HomeAssistantConfiguration DockerResourceConfiguration { get; }

    /// <inheritdoc />
    public override HomeAssistantContainer Build()
    {
        Validate();
        return new HomeAssistantContainer(DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override HomeAssistantBuilder Init()
    {
        return base.Init()
            .WithImage(HomeAssistantImage)
            .WithPortBinding(WebUIPort, true)
            .WithPortBinding(RpcPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(req => req.ForPort(WebUIPort).ForPath("/")));
    }

    /// <inheritdoc />
    protected override void Validate()
    {
        base.Validate();
    }

    /// <inheritdoc />
    protected override HomeAssistantBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override HomeAssistantBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override HomeAssistantBuilder Merge(HomeAssistantConfiguration oldValue, HomeAssistantConfiguration newValue)
    {
        return new HomeAssistantBuilder(new HomeAssistantConfiguration(oldValue, newValue));
    }
}
