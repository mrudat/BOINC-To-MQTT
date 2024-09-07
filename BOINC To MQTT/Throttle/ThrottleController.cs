// <copyright file="ThrottleController.cs" company="Martin Rudat">
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

using System.Threading;
using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Cpu;
using BOINC_To_MQTT.Gpu;
using BOINC_To_MQTT.Mqtt;
using BOINC_To_MQTT.Scaffolding;
using DotNext.Threading;
using Microsoft.Extensions.Options;
using MQTTnet.Protocol;

/// <summary>
/// Interacts with MQTT to throttle GPU/CPU usage on a target BOINC client.
/// </summary>
internal partial class ThrottleController(
    IOptions<Boinc2MqttOptions> options,
    ICpuController cpuController,
    IGpuController gpuController,
    IBoincConnection boincConnection,
    IMqttConnection mqttClient) : IScopedHostedService, IRequiresConfiguration, IHostApplicationBuilderConfiguration
{
    private readonly AsyncLazy<byte[]> configurationPayload = new((ct) => MakeConfigurationPayload(boincConnection, mqttClient, ct));

    private MqttSubscription? subscription = null;

    /// <inheritdoc/>
    public static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IScopedHostedService, ThrottleController>()
            .AddScoped<IRequiresConfiguration, ThrottleController>();
    }

    /// <inheritdoc/>
    async Task IRequiresConfiguration.ConfigureAsync(CancellationToken cancellationToken)
    {
        var boincClientId = await boincConnection.GetHostCrossProjectIdentifierAsync(cancellationToken).ConfigureAwait(false);

        var payload = await this.configurationPayload.WithCancellation(cancellationToken).ConfigureAwait(false);

        await mqttClient.PublishMessage(
            $"{options.Value.MQTT.DiscoveryPrefix}/number/{boincClientId}/config",
            payload,
            cancellationToken: cancellationToken)
        .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var boincClientId = await boincConnection.GetHostCrossProjectIdentifierAsync(cancellationToken).ConfigureAwait(false);

        var throttle = (await boincConnection.GetGlobalPreferencesWorkingAsync(cancellationToken).ConfigureAwait(false)).CpuUsageLimit;

        await this.ApplyThrottle(throttle, cancellationToken).ConfigureAwait(false);

        this.subscription = await mqttClient.SubscribeToTopic($"boinc2mqtt/{boincClientId}/throttle/set", this.ThrottleCallback, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        if (this.subscription != null)
        {
            await this.subscription.DisposeAsync().ConfigureAwait(false);
        }

        this.subscription = null;
    }

    private static async Task<byte[]> MakeConfigurationPayload(IBoincConnection boincConnection, IMqttConnection mqttClient, CancellationToken cancellationToken)
    {
        var boincClientIdTask = boincConnection.GetHostCrossProjectIdentifierAsync(cancellationToken);
        var mqttClientIdTask = mqttClient.GetClientIdentifierAsync(cancellationToken);
        var deviceTask = boincConnection.GetDeviceAsync(cancellationToken);

        await Task.WhenAll(boincClientIdTask, mqttClientIdTask, deviceTask).ConfigureAwait(false);

        var boincClientId = await boincClientIdTask;
        var mqttClientId = await mqttClientIdTask;
        var device = await deviceTask;

        var config = new NumberConfiguration()
        {
            TopicPrefix = $"boinc2mqtt/{boincClientId}/throttle",
            AvailabilityTopic = $"boinc2mqtt/{mqttClientId}/available",
            CommandTopic = "~/set",
            StateTopic = "~",
            Device = device,
            Origin = Origin.Instance,
            UniqueIdentifier = boincClientId,
            Name = "CPU/GPU Throttle",
            QualityOfService = MqttQualityOfServiceLevel.AtLeastOnce,
            Retain = true,
        };

        return config.SerializeToJsonUtf8Bytes();
    }

    private async Task PublishThrottle(double throttle, CancellationToken cancellationToken = default)
    {
        var boincClientId = await boincConnection.GetHostCrossProjectIdentifierAsync(cancellationToken).ConfigureAwait(false);

        await mqttClient.PublishMessage($"boinc2mqtt/{boincClientId}/throttle", throttle.ToString(), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task ThrottleCallback(string topic, string payload, CancellationToken cancellationToken)
    {
        if (!double.TryParse(payload, out var bar))
        {
            return;
        }

        var throttle = bar switch
        {
            > 100 => 100,
            < 10 => 10,
            _ => bar,
        };

        await this.ApplyThrottle(throttle, cancellationToken).ConfigureAwait(false);
    }

    private async Task ApplyThrottle(double throttle, CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            cpuController.UpdateThrottleAsync(throttle, cancellationToken),
            gpuController.UpdateThrottleAsync(throttle, cancellationToken))
        .ConfigureAwait(false);

        await this.PublishThrottle(throttle, cancellationToken).ConfigureAwait(false);
    }
}
