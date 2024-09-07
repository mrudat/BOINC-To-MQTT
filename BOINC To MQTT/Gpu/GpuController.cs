// <copyright file="GpuController.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Gpu;

using System.Diagnostics;
using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Scaffolding;
using BOINC_To_MQTT.Throttle;

/// <inheritdoc cref="IGpuController" />
internal partial class GpuController(
    ILogger<GpuController> logger,
    IBoincContext boincContext,
    IBoincConnection boincConnection,
    TimeProvider timeProvider) : Throttleable, IGpuController, IHostApplicationBuilderConfiguration
{
    private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Workaround for https://github.com/dotnet/runtime/issues/91121.
    /// </summary>
    private readonly ILogger logger = logger;

    /// <inheritdoc/>
    public static void Configure(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IScopedHostedService, GpuController>()
            .AddScoped<IGpuController, GpuController>();
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sleepTime = TimeSpan.FromSeconds(60);
        var modeTime = sleepTime + TimeSpan.FromSeconds(5);

        await this.WaitForThrottleUpdateAsync(stoppingToken).ConfigureAwait(false);

        double averageWorkDone = 1;
        int wasWorking = 1;

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan elapsedTime;
            int didWork;

            if (this.Throttle >= 100)
            {
                await boincConnection.SetGpuModeAsync(BoincRpc.Mode.Auto, modeTime, stoppingToken).ConfigureAwait(false);

                if (wasWorking == 0)
                {
                    this.LogInformationGpuRunning();
                    wasWorking = 1;
                }

                var stopwatch = Stopwatch.StartNew();

                // wait at least sleepTime before changing states again.
                await Task.Delay(sleepTime, timeProvider: timeProvider, cancellationToken: stoppingToken).ConfigureAwait(false);

                await this.WaitForThrottleUpdateAsync(stoppingToken).ConfigureAwait(false);

                stopwatch.Stop();

                elapsedTime = stopwatch.Elapsed;
                didWork = 1;
            }
            else
            {
                if (this.Throttle >= averageWorkDone)
                {
                    await boincConnection.SetGpuModeAsync(BoincRpc.Mode.Auto, modeTime, stoppingToken).ConfigureAwait(false);

                    if (wasWorking == 0)
                    {
                        this.LogInformationGpuRunning();
                        wasWorking = 1;
                    }

                    await Task.Delay(sleepTime, timeProvider: timeProvider, cancellationToken: stoppingToken).ConfigureAwait(false);

                    elapsedTime = sleepTime;
                    didWork = 1;
                }
                else
                {
                    await boincConnection.SetGpuModeAsync(BoincRpc.Mode.Never, modeTime, stoppingToken).ConfigureAwait(false);

                    if (wasWorking == 1)
                    {
                        this.LogInformationGpuPaused();
                        wasWorking = 0;
                    }

                    await Task.Delay(sleepTime, timeProvider: timeProvider, cancellationToken: stoppingToken).ConfigureAwait(false);

                    elapsedTime = sleepTime;
                    didWork = 0;
                }
            }

            if (elapsedTime >= TimeWindow)
            {
                averageWorkDone = didWork;
            }
            else
            {
                var ratio = elapsedTime / TimeWindow;

                averageWorkDone = ((1 - ratio) * averageWorkDone) + (ratio * didWork);
            }
        }
    }

    [LoggerMessage(LogLevel.Information, Message = "GPU paused.", EventId = (int)EventIdentifier.GpuPaused)]
    private partial void LogInformationGpuPaused();

    [LoggerMessage(LogLevel.Information, Message = "GPU running.", EventId = (int)EventIdentifier.GpuRunning)]
    private partial void LogInformationGpuRunning();
}
