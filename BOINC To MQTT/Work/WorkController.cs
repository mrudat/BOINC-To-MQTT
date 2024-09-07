// <copyright file="WorkController.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Work;

using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Mqtt;
using BOINC_To_MQTT.Scaffolding;
using BoincRpc;
using DotNext.Threading;
using Microsoft.Extensions.Options;
using MQTTnet.Protocol;

internal partial class WorkController(
    IOptions<Boinc2MqttOptions> options,
    IMqttConnection mqttClient,
    IBoincConnection boincConnection) : IScopedHostedService, IRequiresConfiguration, IHostApplicationBuilderConfiguration
{
    private readonly AsyncLock allowMoreWorkLock = new();
    private bool allowMoreWork = false;
    private MqttSubscription? subscription = null;

    /// <inheritdoc/>
    public static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IScopedHostedService, WorkController>()
            .AddScoped<IRequiresConfiguration, WorkController>();
    }

    /// <inheritdoc/>
    async Task IRequiresConfiguration.ConfigureAsync(CancellationToken cancellationToken)
    {
        var clientIdTask = boincConnection.GetHostCrossProjectIdentifierAsync(cancellationToken);
        var mqttClientIdTask = mqttClient.GetClientIdentifierAsync(cancellationToken);
        var deviceTask = boincConnection.GetDeviceAsync(cancellationToken);

        await Task.WhenAll(clientIdTask, mqttClientIdTask, deviceTask).ConfigureAwait(false);

        var clientId = await clientIdTask;
        var mqttClientId = await mqttClientIdTask;
        var device = await deviceTask;

        // TODO make this lazy?
        var config = new SwitchConfiguration()
        {
            TopicPrefix = $"boinc2mqtt/{clientId}/moreWork",
            AvailabilityTopic = $"boinc2mqtt/{mqttClientId}/available",
            CommandTopic = "~/set",
            StateTopic = "~",
            Device = device,
            Origin = Origin.Instance,
            UniqueIdentifier = clientId,
            Name = "Allow More Work",
            QualityOfService = MqttQualityOfServiceLevel.AtLeastOnce,
            Retain = true,
        };

        var configPayload = config.SerializeToJsonUtf8Bytes();

        await mqttClient.PublishMessage(
            $"{options.Value.MQTT.DiscoveryPrefix}/switch/{clientId}/config",
            configPayload,
            cancellationToken: cancellationToken)
        .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        var clientId = await boincConnection.GetHostCrossProjectIdentifierAsync(cancellationToken).ConfigureAwait(false);

        this.allowMoreWork = Array.TrueForAll(await boincConnection.GetProjectStatusAsync(cancellationToken).ConfigureAwait(false), MoreWorkAllowed);

        await this.PublishAllowMoreWork(cancellationToken).ConfigureAwait(false);

        this.subscription = await mqttClient.SubscribeToTopic($"boinc2mqtt/{clientId}/moreWork/set", this.EnableCallback, cancellationToken).ConfigureAwait(false);
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

    private static bool MoreWorkAllowed(Project project) => !project.DontRequestMoreWork;

    private async Task EnableCallback(string topic, string payload, CancellationToken cancellationToken)
    {
        if (payload.Length <= 1 || payload.Length > 3)
        {
            return;
        }

        bool newAllowMoreWork;

        if (payload.Equals("ON", StringComparison.OrdinalIgnoreCase))
        {
            newAllowMoreWork = true;
        }
        else if (payload.Equals("OFF", StringComparison.OrdinalIgnoreCase))
        {
            newAllowMoreWork = false;
        }
        else
        {
            return;
        }

        using var asyncLockHolder = await this.allowMoreWorkLock.AcquireAsync(cancellationToken).ConfigureAwait(false);

        if (this.allowMoreWork == newAllowMoreWork)
        {
            return;
        }

        var operation = this.allowMoreWork ? ProjectOperation.AllowMoreWork : ProjectOperation.NoMoreWork;

        var projects = await boincConnection.GetProjectStatusAsync(cancellationToken).ConfigureAwait(false);

        foreach (var project in projects.Where(project => MoreWorkAllowed(project) != this.allowMoreWork))
        {
            await boincConnection.PerformProjectOperationAsync(project, operation, cancellationToken).ConfigureAwait(false);
        }

        await this.PublishAllowMoreWork(cancellationToken);

        this.allowMoreWork = newAllowMoreWork;
    }

    private async Task PublishAllowMoreWork(CancellationToken cancellationToken = default)
    {
        var clientId = await boincConnection.GetHostCrossProjectIdentifierAsync(cancellationToken).ConfigureAwait(false);

        await mqttClient.PublishMessage($"boinc2mqtt/{clientId}/moreWork", this.allowMoreWork ? "NO" : "YES", cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
