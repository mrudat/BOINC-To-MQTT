// <copyright file="EventIdentifier.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT;

using BOINC_To_MQTT.Cpu;
using BOINC_To_MQTT.Gpu;
using BOINC_To_MQTT.Mqtt;

/// <summary>
/// EventIdentifiers for log messages.
/// </summary>
public enum EventIdentifier
{
    /// <summary>
    /// <see cref="MqttConnection.LogInformationConnected"/>.
    /// </summary>
    InformationConnectedToMQTTServer,

    /// <summary>
    /// <see cref="MqttConnection.LogInformationDisconnected"/>.
    /// </summary>
    InformationDisconnectedFromMQTTServer,

    /// <summary>
    /// <see cref="CpuController.LogInformationNewCPUThrottleSetting(double)"/>.
    /// </summary>
    NewCPUThrottleSetting,

    /// <summary>
    /// <see cref="GpuController.LogInformationPausedGUPWorkload(uint, double)"/>.
    /// </summary>
    GpuPaused,

    /// <summary>
    /// <see cref="MqttConnection.LogWarningUnhandledTopic(string)"/>.
    /// </summary>
    WarningUnhandledTopic,

    /// <summary>
    /// <see cref="GpuController.LogInformationGpuRunning"/>.
    /// </summary>
    GpuRunning,

    /// <summary>
    /// <see cref="BoincWorkerFactory.LogErrorFailedToStartBoincWorker(ILogger{BoincWorkerFactory}, Exception)"/>.
    /// </summary>
    ErrorFailedToStartBoincWorker,

    /// <summary>
    /// <see cref="MqttConnection.LogDebugMessageRecieved(string, string)"/>.
    /// </summary>
    DebugMessageRecieved,

    /// <summary>
    /// <see cref="MqttConnection.LogErrorProcessingMessage(Exception)"/>.
    /// </summary>
    ErrorProcessingMessage,

    /// <summary>
    /// <see cref="MqttConnection.LogDebugSendingMessage(string, byte[])"/> and
    /// <see cref="MqttConnection.LogDebugSendingMessage(string, string)"/>.
    /// </summary>
    DebugSendingMessage,
}
