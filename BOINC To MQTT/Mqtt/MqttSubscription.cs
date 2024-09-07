// <copyright file="MqttSubscription.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Mqtt;

/// <summary>
/// A subscription to messages on <paramref name="Topic"/>.
/// </summary>
/// <param name="Topic">The topic being subscribed to.</param>
/// <param name="SubscriptionCallback">The callback to accept the message.</param>
/// <param name="UnsubscribeCallback">A callback to unsubscribe from the topic.</param>
internal record MqttSubscription(
    string Topic,
    SubscriptionCallback SubscriptionCallback,
    UnubscribeCallback UnsubscribeCallback) : IAsyncDisposable
{
    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.UnsubscribeCallback(this).ConfigureAwait(false);
    }
}

/// <summary>
/// A callback to accept a <paramref name="message"/> on <paramref name="topic"/>.
/// </summary>
/// <param name="topic">The topic that the <paramref name="message"/> was sent on.</param>
/// <param name="message">The message that was received on <paramref name="topic"/>.</param>
/// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
internal delegate Task SubscriptionCallback(string topic, string message, CancellationToken cancellationToken = default);

/// <summary>
/// A callback to unsubscribe from a topic.
/// </summary>
/// <param name="subscription">The <see cref="MqttSubscription"/> to unsubscribe from.</param>
/// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
internal delegate Task UnubscribeCallback(MqttSubscription subscription, CancellationToken cancellationToken = default);
