// <copyright file="MosquittoConfiguration.cs" company="Martin Rudat">
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

namespace Testcontainers.Mosquitto;

using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

/// <summary>
/// The configuration for an Apache Mosquitto container.
/// </summary>
public class MosquittoConfiguration : ContainerConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MosquittoConfiguration"/> class.
    /// </summary>
    /// <param name="userName">The user name for the default user.</param>
    /// <param name="password">The password for the default user.</param>
    public MosquittoConfiguration(
        string? userName = null,
        string? password = null)
    {
        this.UserName = userName;
        this.Password = password;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MosquittoConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public MosquittoConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MosquittoConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker container configuration.</param>
    public MosquittoConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MosquittoConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Mosquitto container configuration.</param>
    public MosquittoConfiguration(MosquittoConfiguration resourceConfiguration)
        : this(new MosquittoConfiguration(), resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MosquittoConfiguration" /> class.
    /// </summary>
    /// <param name="oldValue">The old Mosquitto container configuration.</param>
    /// <param name="newValue">The new Mosquitto container configuration.</param>
    public MosquittoConfiguration(MosquittoConfiguration oldValue, MosquittoConfiguration newValue)
        : base(oldValue, newValue)
    {
        this.UserName = BuildConfiguration.Combine(oldValue.UserName, newValue.UserName);
        this.Password = BuildConfiguration.Combine(oldValue.Password, newValue.Password);
    }

    /// <summary>
    /// Gets the password for the default user.
    /// </summary>
    public string? Password { get; }

    /// <summary>
    /// Gets the user name for the default user.
    /// </summary>
    public string? UserName { get; }

    /// <summary>
    /// Gets the set of users that have been added to this instance of Mosquitto.
    /// </summary>
    public Dictionary<string, string> Users { get; } = [];
}
