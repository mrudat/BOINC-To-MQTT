// Ignore Spelling: RPC

using Docker.DotNet.Models;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace Testcontainers.HomeAssistant;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class HomeAssistantBuilder : ContainerBuilder<HomeAssistantBuilder, HomeAssistantContainer, HomeAssistantConfiguration>
{
    public const string HomeAssistantImage = "ghcr.io/home-assistant/home-assistant:2024.5.3";

    public const string HostName = "home-assistant";

    public const string DefaultOwnerDisplayName = "The Owner";

    public const string DefaultOwnerUserName = "theOwner";

    public const string DefaultOwnerPassword = "theOwner's password";

    public const string DefaultLanguage = "en";

    public const string DefaultTimeZone = "Etc/UTC";

    public const string DefaultCurrency = "USD";

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
            .WithOwnerDisplayName(DefaultOwnerDisplayName)
            .WithOwnerUserName(DefaultOwnerUserName)
            .WithOwnerPassword(DefaultOwnerPassword)
            .WithLanguage(DefaultLanguage)
            .WithTimeZone(DefaultTimeZone)
            .WithCurrency(DefaultCurrency)
            .WithPortBinding(WebUIPort, true)
            .WithPortBinding(RpcPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(req => req.ForPort(WebUIPort).ForPath("/")));
    }

    /// <summary>
    /// Sets the owner's display name to <paramref name="ownerDisplayName"/>.
    /// </summary>
    /// <param name="ownerDisplayName">The new owner's display name.</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/></returns>
    public HomeAssistantBuilder WithOwnerDisplayName(string ownerDisplayName)
    {
        return Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(ownerDisplayName: ownerDisplayName));
    }

    /// <summary>
    /// Sets the owner's user name to <paramref name="ownerUserName"/>.
    /// </summary>
    /// <param name="ownerUserName">The new owner's user name.</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/></returns>
    public HomeAssistantBuilder WithOwnerUserName(string ownerUserName)
    {
        return Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(ownerUserName: ownerUserName));
    }

    /// <summary>
    /// Sets the owner's password to <paramref name="ownerPassword"/>.
    /// </summary>
    /// <param name="ownerPassword">The new owner's password.</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/></returns>
    public HomeAssistantBuilder WithOwnerPassword(string ownerPassword)
    {
        return Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(ownerPassword: ownerPassword));
    }

    /// <summary>
    /// Sets the language to <paramref name="language"/>.
    /// </summary>
    /// <param name="language">The new language.</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/></returns>
    public HomeAssistantBuilder WithLanguage(string language)
    {
        return Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(language: language));
    }

    /// <summary>
    /// Sets the time zone to <paramref name="timeZone"/>.
    /// </summary>
    /// <param name="timeZone">The new time zone.</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/></returns>
    public HomeAssistantBuilder WithTimeZone(string timeZone)
    {
        return Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(timeZone: timeZone));
    }

    /// <summary>
    /// Sets the currency to <paramref name="currency"/>.
    /// </summary>
    /// <param name="currency">The new currency as an ISO currency code (eg. USD).</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/></returns>
    public HomeAssistantBuilder WithCurrency(string currency)
    {
        return Merge(DockerResourceConfiguration, new HomeAssistantConfiguration(currency: currency));
    }

    /// <inheritdoc />
    protected override void Validate()
    {
        base.Validate();

        _ = Guard.Argument(DockerResourceConfiguration.OwnerDisplayName, nameof(DockerResourceConfiguration.OwnerDisplayName))
            .NotNull();

        _ = Guard.Argument(DockerResourceConfiguration.OwnerUserName, nameof(DockerResourceConfiguration.OwnerUserName))
            .NotNull()
            .NotEmpty();

        _ = Guard.Argument(DockerResourceConfiguration.OwnerPassword, nameof(DockerResourceConfiguration.OwnerPassword))
            .NotNull()
            .NotEmpty();

        _ = Guard.Argument(DockerResourceConfiguration.TimeZone, nameof(DockerResourceConfiguration.TimeZone))
            .NotNull()
            .NotEmpty();

        _ = Guard.Argument(DockerResourceConfiguration.Currency, nameof(DockerResourceConfiguration.Currency))
            .NotNull()
            .NotEmpty();
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
