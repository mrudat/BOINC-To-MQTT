// Ignore Spelling: RPC

using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;

namespace TestContainers.BOINC;

/// <inheritdoc cref="ContainerConfiguration" />
public class BOINCConfiguration : ContainerConfiguration
{
    public BOINCConfiguration() { }

    public BOINCConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) : base(resourceConfiguration) { }

    public BOINCConfiguration(IContainerConfiguration resourceConfiguration) : base(resourceConfiguration) { }

    public BOINCConfiguration(BOINCConfiguration resourceConfiguration) : this(new BOINCConfiguration(), resourceConfiguration) { }

    public BOINCConfiguration(BOINCConfiguration oldValue, BOINCConfiguration newValue) : base(oldValue, newValue) { }
}