// <copyright file="CpuController.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Cpu;

using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Scaffolding;
using BOINC_To_MQTT.Throttle;

/// <inheritdoc cref="ICpuController" />
internal partial class CpuController(
    ILogger<CpuController> logger,
    IBoincConnection boincConnection,
    TimeProvider timeProvider) : Throttleable, ICpuController, IHostApplicationBuilderConfiguration
{
    // workaround for https://github.com/dotnet/runtime/issues/91121
    private readonly ILogger logger = logger;

    /// <inheritdoc/>
    public static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IScopedHostedService, CpuController>()
            .AddScoped<ICpuController, CpuController>();
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var throttle = await this.WaitForThrottleUpdateAsync(stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            await this.ApplyCPUThrottle(throttle, stoppingToken).ConfigureAwait(false);

            this.LogInformationNewCPUThrottleSetting(throttle);

            await Task.Delay(TimeSpan.FromSeconds(30), timeProvider: timeProvider, cancellationToken: stoppingToken).ConfigureAwait(false);

            throttle = await this.WaitForThrottleUpdateAsync(stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ApplyCPUThrottle(double throttle, CancellationToken cancellationToken = default)
    {
        var globalPreferencesOverride = await boincConnection.GetGlobalPreferencesOverrideAsync(cancellationToken).ConfigureAwait(false);

        globalPreferencesOverride.SetElementValue("cpu_usage_limit", throttle.ToString());

        await boincConnection.SetGlobalPreferencesOverrideAsync(globalPreferencesOverride, cancellationToken).ConfigureAwait(false);

        await boincConnection.ReadGlobalPreferencesOverrideAsync(cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(LogLevel.Information, Message = "New CPU throttle setting: {throttle}", EventId = (int)EventIdentifier.NewCPUThrottleSetting)]
    private partial void LogInformationNewCPUThrottleSetting(double throttle);
}
