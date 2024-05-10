using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;

namespace Testcontainers.HomeAssistant;

/// <inheritdoc cref="ContainerConfiguration" />
public class HomeAssistantConfiguration : ContainerConfiguration
{
    public HomeAssistantConfiguration() { }

    public HomeAssistantConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) : base(resourceConfiguration) { }

    public HomeAssistantConfiguration(IContainerConfiguration resourceConfiguration) : base(resourceConfiguration) { }

    public HomeAssistantConfiguration(HomeAssistantConfiguration resourceConfiguration) : this(new HomeAssistantConfiguration(), resourceConfiguration) { }

    public HomeAssistantConfiguration(HomeAssistantConfiguration oldValue, HomeAssistantConfiguration newValue) : base(oldValue, newValue) { }
}
