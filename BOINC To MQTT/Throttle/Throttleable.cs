// <copyright file="IThrottleable.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Throttle;

using BOINC_To_MQTT.Scaffolding;

/// <inheritdoc cref="IThrottleable"/>
internal abstract class Throttleable : ScopedBackgroundService, IThrottleable
{
    private double throttle = 0;
    private AsyncTaskCompletionSource throttleHasChanged = new();

    /// <summary>
    /// Gets the current value of the throttle.<br/>
    /// Not valid until after the first call to <see cref="IThrottleable.UpdateThrottleAsync(double, CancellationToken)"/>/<see cref="WaitForThrottleUpdateAsync(CancellationToken)"/>.
    /// </summary>
    protected double Throttle => this.throttle;

    /// <inheritdoc/>
    Task IThrottleable.UpdateThrottleAsync(double throttle, CancellationToken cancellationToken)
    {
        this.throttle = throttle;
        return this.throttleHasChanged.TrySetResult();
    }

    /// <summary>
    /// Returns a <see cref="Task{double}"/> that completes when a new throttle value is available and includes the value of the <see cref="Throttle"/> at the time of completion.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A <see cref="Task{double}"/> that completes when a new throttle value is available.</returns>
    protected async Task<double> WaitForThrottleUpdateAsync(CancellationToken cancellationToken)
    {
        await this.throttleHasChanged.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        this.throttleHasChanged = new();
        return this.throttle;
    }
}
