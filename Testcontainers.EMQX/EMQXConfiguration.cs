// Ignore Spelling: MQTT username EMQX

using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;

namespace Testcontainers.EMQX;

/// <inheritdoc cref="ContainerConfiguration" />
public class EmqxConfiguration : ContainerConfiguration
{
    public EmqxConfiguration() { }

    public EmqxConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) : base(resourceConfiguration) { }

    public EmqxConfiguration(IContainerConfiguration resourceConfiguration) : base(resourceConfiguration) { }

    public EmqxConfiguration(EmqxConfiguration resourceConfiguration) : this(new EmqxConfiguration(), resourceConfiguration) { }

    public EmqxConfiguration(EmqxConfiguration oldValue, EmqxConfiguration newValue) : base(oldValue, newValue) { }
}
