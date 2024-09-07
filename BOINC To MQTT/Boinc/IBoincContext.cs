// <copyright file="IBoincContext.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Boinc;

/// <summary>
/// Passes on <see cref="CommonBoincOptions"/> to the scope handling the BOINC client.
/// </summary>
internal interface IBoincContext
{
    /// <summary>
    /// Gets the <see cref="CommonBoincOptions"/> for the BOINC client.
    /// </summary>
    internal CommonBoincOptions Options { get; }

    /// <summary>
    /// Gets a name for the BOINC client.
    /// </summary>
    /// <returns>A name for the BOINC client.</returns>
    internal string GetName();

    /// <summary>
    /// Gets a description of the associated BOINC client.
    /// </summary>
    /// <returns>A description of the associated BOINC client.</returns>
    internal string GetUserReadableDescription();
}
