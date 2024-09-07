// <copyright file="IRequiresConfiguration.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Scaffolding;

/// <summary>
/// Interface for classes that can publish a Home Assistant MQTT configuration message.
/// </summary>
internal interface IRequiresConfiguration
{
    /// <summary>
    /// Called when Home Assistant's MQTT integration comes online in order to publish a configuration message.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal Task ConfigureAsync(CancellationToken cancellationToken = default);
}
