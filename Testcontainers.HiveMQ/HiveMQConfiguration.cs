// Ignore Spelling: MQTT username

using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;

namespace Testcontainers.HiveMQ;

/// <inheritdoc cref="ContainerConfiguration" />
public class HiveMQConfiguration : ContainerConfiguration
{
    public HiveMQConfiguration() { }

    public HiveMQConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) : base(resourceConfiguration) { }

    public HiveMQConfiguration(IContainerConfiguration resourceConfiguration) : base(resourceConfiguration) { }

    public HiveMQConfiguration(HiveMQConfiguration resourceConfiguration) : this(new HiveMQConfiguration(), resourceConfiguration) { }

    public HiveMQConfiguration(HiveMQConfiguration oldValue, HiveMQConfiguration newValue) : base(oldValue, newValue) { }
}
