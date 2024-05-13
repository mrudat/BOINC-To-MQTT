// Ignore Spelling: RPC BOINC

using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace TestContainers.BOINC;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class BoincBuilder : ContainerBuilder<BoincBuilder, BoincContainer, BoincConfiguration>
{
    public const string BOINCImage = "boinc/client:latest";

    public const ushort GuiRpcPort = 31416;

    public BoincBuilder() : this(new BoincConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    private BoincBuilder(BoincConfiguration resourceConfiguration) : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override BoincConfiguration DockerResourceConfiguration { get; }

    /// <inheritdoc />
    public override BoincContainer Build()
    {
        Validate();
        return new BoincContainer(DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override BoincBuilder Init()
    {
        return base.Init()
            .WithImage(BOINCImage)
            .WithEnvironment("BOINC_CMD_LINE_OPTIONS", "--allow_remote_gui_rpc")
            .WithPortBinding(GuiRpcPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Initialization completed"));
    }

    /// <inheritdoc />
    protected override void Validate()
    {
        base.Validate();
    }

    /// <inheritdoc />
    protected override BoincBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new BoincConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override BoincBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new BoincConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override BoincBuilder Merge(BoincConfiguration oldValue, BoincConfiguration newValue)
    {
        return new BoincBuilder(new BoincConfiguration(oldValue, newValue));
    }
}
