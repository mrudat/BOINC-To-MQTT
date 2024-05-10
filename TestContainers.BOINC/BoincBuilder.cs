// Ignore Spelling: RPC BOINC

using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace TestContainers.BOINC;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class BoincBuilder : ContainerBuilder<BoincBuilder, BOINCContainer, BOINCConfiguration>
{
    public const string BOINCImage = "boinc/client:latest";

    public const ushort GuiRpcPort = 31416;

    public BoincBuilder() : this(new BOINCConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    private BoincBuilder(BOINCConfiguration resourceConfiguration) : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override BOINCConfiguration DockerResourceConfiguration { get; }

    /// <inheritdoc />
    public override BOINCContainer Build()
    {
        Validate();
        return new BOINCContainer(DockerResourceConfiguration);
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
        return Merge(DockerResourceConfiguration, new BOINCConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override BoincBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new BOINCConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override BoincBuilder Merge(BOINCConfiguration oldValue, BOINCConfiguration newValue)
    {
        return new BoincBuilder(new BOINCConfiguration(oldValue, newValue));
    }
}
