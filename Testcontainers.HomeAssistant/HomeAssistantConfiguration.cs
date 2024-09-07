// <copyright file="HomeAssistantConfiguration.cs" company="Martin Rudat">
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

namespace Testcontainers.HomeAssistant;

using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

/// <inheritdoc cref="ContainerConfiguration" />
public class HomeAssistantConfiguration : ContainerConfiguration
{
    public string? Country;
    public string? Currency;
    public string? Language;
    public string? OwnerDisplayName;

    public string? OwnerPassword;
    public string? OwnerUserName;
    public string? TimeZone;

    public HomeAssistantConfiguration(
        string? ownerDisplayName = null,
        string? ownerUserName = null,
        string? ownerPassword = null,
        string? language = null,
        string? timeZone = null,
        string? currency = null,
        string? country = null)
    {
        this.OwnerDisplayName = ownerDisplayName;
        this.OwnerUserName = ownerUserName;
        this.OwnerPassword = ownerPassword;
        this.Language = language;
        this.TimeZone = timeZone;
        this.Currency = currency;
        this.Country = country;
    }

    public HomeAssistantConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    public HomeAssistantConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    public HomeAssistantConfiguration(HomeAssistantConfiguration resourceConfiguration)
        : this(new HomeAssistantConfiguration(), resourceConfiguration)
    {
    }

    public HomeAssistantConfiguration(HomeAssistantConfiguration oldValue, HomeAssistantConfiguration newValue)
        : base(oldValue, newValue)
    {
        this.OwnerDisplayName = BuildConfiguration.Combine(oldValue.OwnerDisplayName, newValue.OwnerDisplayName);
        this.OwnerUserName = BuildConfiguration.Combine(oldValue.OwnerUserName, newValue.OwnerUserName);
        this.OwnerPassword = BuildConfiguration.Combine(oldValue.OwnerPassword, newValue.OwnerPassword);
        this.Language = BuildConfiguration.Combine(oldValue.Language, newValue.Language);
        this.TimeZone = BuildConfiguration.Combine(oldValue.TimeZone, newValue.TimeZone);
        this.Currency = BuildConfiguration.Combine(oldValue.Currency, newValue.Currency);
        this.Country = BuildConfiguration.Combine(oldValue.Country, newValue.Country);
    }
}
