// <copyright file="AuthorisationFailedException.cs" company="Martin Rudat">
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

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a failure to authenticate to the <paramref name="boincContext"/>.
/// </summary>
/// <param name="boincContext">The <see cref="BoincContext"/> that failed to authenticate.</param>
[Serializable]
[SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "This exception is handled internally.")]
internal class AuthorisationFailedException(IBoincContext boincContext) : Exception($"Failed to authenticate to BOINC client {boincContext.GetUserReadableDescription()}")
{
}
