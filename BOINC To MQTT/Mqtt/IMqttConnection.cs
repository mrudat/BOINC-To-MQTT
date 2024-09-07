// <copyright file="IMqttConnection.cs" company="Martin Rudat">
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
/// An active connection to a MQTT server.
/// </summary>
internal interface IMqttConnection
{
    /// <summary>
    /// Gets the client identifier for this connection.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    internal Task<string> GetClientIdentifierAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes the <paramref name="message"/> to <paramref name="topic"/>.
    /// </summary>
    /// <param name="topic">The topic to send the <paramref name="message"/> to.</param>
    /// <param name="message">The payload to send to the <paramref name="topic"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    internal Task PublishMessage(string topic, byte[] message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes the <paramref name="message"/> to <paramref name="topic"/>.
    /// </summary>
    /// <param name="topic">The topic to send the <paramref name="message"/> to.</param>
    /// <param name="message">The payload to send to the <paramref name="topic"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    internal Task PublishMessage(string topic, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to messages sent to <paramref name="topic"/>.
    /// </summary>
    /// <param name="topic">The topic to subscribe to.</param>
    /// <param name="callback">A <see cref="SubscriptionCallback"/> to receive the message.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to abort the operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    internal Task<MqttSubscription> SubscribeToTopic(string topic, SubscriptionCallback callback, CancellationToken cancellationToken = default);
}
