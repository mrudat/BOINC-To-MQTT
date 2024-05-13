// Ignore Spelling: RPC BOINC

using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;

namespace TestContainers.BOINC;

/// <inheritdoc cref="ContainerConfiguration" />
public class BoincConfiguration : ContainerConfiguration
{
    public BoincConfiguration() { }

    public BoincConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) : base(resourceConfiguration) { }

    public BoincConfiguration(IContainerConfiguration resourceConfiguration) : base(resourceConfiguration) { }

    public BoincConfiguration(BoincConfiguration resourceConfiguration) : this(new BoincConfiguration(), resourceConfiguration) { }

    public BoincConfiguration(BoincConfiguration oldValue, BoincConfiguration newValue) : base(oldValue, newValue) { }
}