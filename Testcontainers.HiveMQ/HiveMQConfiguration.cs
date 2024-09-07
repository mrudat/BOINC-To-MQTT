// <copyright file="HiveMQConfiguration.cs" company="Martin Rudat">
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

namespace Testcontainers.HiveMQ;

using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;

/// <inheritdoc cref="ContainerConfiguration" />
public class HiveMQConfiguration : ContainerConfiguration
{
    public HiveMQConfiguration()
    {
    }

    public HiveMQConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    public HiveMQConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    public HiveMQConfiguration(HiveMQConfiguration resourceConfiguration)
        : this(new HiveMQConfiguration(), resourceConfiguration)
    {
    }

    public HiveMQConfiguration(HiveMQConfiguration oldValue, HiveMQConfiguration newValue)
        : base(oldValue, newValue)
    {
    }
}
