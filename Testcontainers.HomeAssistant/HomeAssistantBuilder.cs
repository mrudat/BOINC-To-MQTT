// <copyright file="HomeAssistantBuilder.cs" company="Martin Rudat">
// BOINC To MQTT - Exposes some BOINC controls via MQTT for integration with Home Assistant.
// Copyright (C) 2024  Martin Rudat
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>

// Ignore Spelling: RPC username Initializes
namespace Testcontainers.HomeAssistant;

using Docker.DotNet.Models;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}"/>
public sealed partial class HomeAssistantBuilder : ContainerBuilder<HomeAssistantBuilder, HomeAssistantContainer, HomeAssistantConfiguration>
{
    // FIXME find a value that works
    public const string DefaultCountry = "United Kingdom";

    // FIXME find a value that works
    public const string DefaultCurrency = "USD";

    public const string DefaultLanguage = "en";
    public const string DefaultOwnerDisplayName = "The Owner";
    public const string DefaultOwnerPassword = "password";
    public const string DefaultOwnerUserName = "username";

    // FIXME find a value that works
    public const string DefaultTimeZone = "(GMT+00:00) GMT (no daylight saving)";

    public const string HomeAssistantImage = "ghcr.io/home-assistant/home-assistant:2024.5.4";

    public const string HostName = "home-assistant";
    public const ushort RpcPort = 40000;
    public const ushort WebUIPort = 8123;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeAssistantBuilder"/> class.
    /// </summary>
    public HomeAssistantBuilder()
        : this(new HomeAssistantConfiguration())
    {
        this.DockerResourceConfiguration = this.Init().DockerResourceConfiguration;
    }

    private HomeAssistantBuilder(HomeAssistantConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        this.DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override HomeAssistantConfiguration DockerResourceConfiguration { get; }

    /// <inheritdoc />
    public override HomeAssistantContainer Build()
    {
        this.Validate();

        return new HomeAssistantContainer(this.DockerResourceConfiguration);
    }

    /// <summary>
    /// Sets the country to <paramref name="country"/>.
    /// </summary>
    /// <param name="country">The new country.</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/>.</returns>
    public HomeAssistantBuilder WithCountry(string country)
    {
        return this.Merge(this.DockerResourceConfiguration, new HomeAssistantConfiguration(country: country));
    }

    /// <summary>
    /// Sets the currency to <paramref name="currency"/>.
    /// </summary>
    /// <param name="currency">The new currency as an ISO currency code (eg. USD).</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/>.</returns>
    public HomeAssistantBuilder WithCurrency(string currency)
    {
        return this.Merge(this.DockerResourceConfiguration, new HomeAssistantConfiguration(currency: currency));
    }

    /// <summary>
    /// Sets the language to <paramref name="language"/>.
    /// </summary>
    /// <param name="language">The new language.</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/>.</returns>
    public HomeAssistantBuilder WithLanguage(string language)
    {
        return this.Merge(this.DockerResourceConfiguration, new HomeAssistantConfiguration(language: language));
    }

    /// <summary>
    /// Sets the owner's display name to <paramref name="ownerDisplayName"/>.
    /// </summary>
    /// <param name="ownerDisplayName">The new owner's display name.</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/>.</returns>
    public HomeAssistantBuilder WithOwnerDisplayName(string ownerDisplayName)
    {
        return this.Merge(this.DockerResourceConfiguration, new HomeAssistantConfiguration(ownerDisplayName: ownerDisplayName));
    }

    /// <summary>
    /// Sets the owner's password to <paramref name="ownerPassword"/>.
    /// </summary>
    /// <param name="ownerPassword">The new owner's password.</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/>.</returns>
    public HomeAssistantBuilder WithOwnerPassword(string ownerPassword)
    {
        return this.Merge(this.DockerResourceConfiguration, new HomeAssistantConfiguration(ownerPassword: ownerPassword));
    }

    /// <summary>
    /// Sets the owner's user name to <paramref name="ownerUserName"/>.
    /// </summary>
    /// <param name="ownerUserName">The new owner's user name.</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/>.</returns>
    public HomeAssistantBuilder WithOwnerUserName(string ownerUserName)
    {
        return this.Merge(this.DockerResourceConfiguration, new HomeAssistantConfiguration(ownerUserName: ownerUserName));
    }

    /// <summary>
    /// Sets the time zone to <paramref name="timeZone"/>.
    /// </summary>
    /// <param name="timeZone">The new time zone.</param>
    /// <returns>A configured instance of <see cref="HomeAssistantBuilder"/>.</returns>
    public HomeAssistantBuilder WithTimeZone(string timeZone)
    {
        return this.Merge(this.DockerResourceConfiguration, new HomeAssistantConfiguration(timeZone: timeZone));
    }

    /// <inheritdoc />
    protected override HomeAssistantBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return this.Merge(this.DockerResourceConfiguration, new HomeAssistantConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override HomeAssistantBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return this.Merge(this.DockerResourceConfiguration, new HomeAssistantConfiguration(resourceConfiguration));
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
            .WithCountry(DefaultCountry)
            .WithPortBinding(WebUIPort, true)
            .WithPortBinding(RpcPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(req => req.ForPort(WebUIPort).ForPath("/")));
    }

    /// <inheritdoc />
    protected override HomeAssistantBuilder Merge(HomeAssistantConfiguration oldValue, HomeAssistantConfiguration newValue)
    {
        return new HomeAssistantBuilder(new HomeAssistantConfiguration(oldValue, newValue));
    }

    /// <inheritdoc />
    protected override void Validate()
    {
        base.Validate();

        _ = Guard.Argument(this.DockerResourceConfiguration.OwnerDisplayName, nameof(this.DockerResourceConfiguration.OwnerDisplayName))
            .NotNull();

        _ = Guard.Argument(this.DockerResourceConfiguration.OwnerUserName, nameof(this.DockerResourceConfiguration.OwnerUserName))
            .NotNull()
            .NotEmpty();

        _ = Guard.Argument(this.DockerResourceConfiguration.OwnerPassword, nameof(this.DockerResourceConfiguration.OwnerPassword))
            .NotNull()
            .NotEmpty();

        _ = Guard.Argument(this.DockerResourceConfiguration.TimeZone, nameof(this.DockerResourceConfiguration.TimeZone))
            .NotNull()
            .NotEmpty();

        _ = Guard.Argument(this.DockerResourceConfiguration.Currency, nameof(this.DockerResourceConfiguration.Currency))
            .NotNull()
            .NotEmpty();
    }
}
