using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace Testcontainers.HomeAssistant;

/// <inheritdoc cref="ContainerConfiguration" />
public class HomeAssistantConfiguration : ContainerConfiguration
{
    public string? OwnerDisplayName;

    public string? OwnerUserName;

    public string? OwnerPassword;

    public string? Language;

    public string? TimeZone;

    public string? Currency;

    public HomeAssistantConfiguration(
        string? ownerDisplayName = null,
        string? ownerUserName = null,
        string? ownerPassword = null,
        string? language = null,
        string? timeZone = null,
        string? currency = null)
    {
        OwnerDisplayName = ownerDisplayName;
        OwnerUserName = ownerUserName;
        OwnerPassword = ownerPassword;
        Language = language;
        TimeZone = timeZone;
        Currency = currency;
    }

    public HomeAssistantConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) : base(resourceConfiguration) { }

    public HomeAssistantConfiguration(IContainerConfiguration resourceConfiguration) : base(resourceConfiguration) { }

    public HomeAssistantConfiguration(HomeAssistantConfiguration resourceConfiguration) : this(new HomeAssistantConfiguration(), resourceConfiguration) { }

    public HomeAssistantConfiguration(HomeAssistantConfiguration oldValue, HomeAssistantConfiguration newValue) : base(oldValue, newValue)
    {
        OwnerDisplayName = BuildConfiguration.Combine(oldValue.OwnerDisplayName, newValue.OwnerDisplayName);
        OwnerUserName = BuildConfiguration.Combine(oldValue.OwnerUserName, newValue.OwnerUserName);
        OwnerPassword = BuildConfiguration.Combine(oldValue.OwnerPassword, newValue.OwnerPassword);
        Language = BuildConfiguration.Combine(oldValue.Language, newValue.Language);
        TimeZone = BuildConfiguration.Combine(oldValue.TimeZone, newValue.TimeZone);
        Currency = BuildConfiguration.Combine(oldValue.Currency, newValue.Currency);
    }
}
