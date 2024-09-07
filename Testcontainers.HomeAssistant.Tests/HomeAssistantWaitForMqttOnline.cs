// <copyright file="HomeAssistantWaitForMqttOnline.cs" company="Martin Rudat">
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

namespace Testcontainers.HomeAssistant.Tests;

using MQTTnet;
using MQTTnet.Client;

internal class HomeAssistantWaitForMqttOnline : IAsyncDisposable
{
    private readonly MqttFactory mqttFactory;

    private readonly IMqttClient mqttClient;

    private readonly TaskCompletionSource taskCompletionSource = new();

    internal HomeAssistantWaitForMqttOnline()
    {
        this.mqttFactory = new MqttFactory();

        this.mqttClient = this.mqttFactory.CreateMqttClient();
    }

    internal async Task StartAsync(Uri mqttUri, CancellationToken cancellationToken = default)
    {
        this.mqttClient.ApplicationMessageReceivedAsync += this.WaitForMessage;
        var mqttClientOptions = this.mqttFactory.CreateClientOptionsBuilder()
            .WithConnectionUri(mqttUri)
            .Build();
        await this.mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);

        await this.mqttClient.SubscribeAsync("homeassistant/status", cancellationToken: cancellationToken);
    }

    private async Task WaitForMessage(MqttApplicationMessageReceivedEventArgs args)
    {
        if (args.ApplicationMessage.Topic != "homeassistant/status")
        {
            return;
        }

        if (!args.ApplicationMessage.ConvertPayloadToString().Equals("online"))
        {
            return;
        }

        this.mqttClient.ApplicationMessageReceivedAsync -= this.WaitForMessage;
        this.taskCompletionSource.TrySetResult();
        await Task.Yield();
        await this.mqttClient.DisconnectAsync();
    }

    internal async Task Wait()
    {
        await this.taskCompletionSource.Task;
        await Task.Yield();
    }

    public ValueTask DisposeAsync()
    {
        this.mqttClient.Dispose();
        return ValueTask.CompletedTask;
    }
}
