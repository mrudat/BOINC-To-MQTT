// <copyright file="Program.cs" company="Martin Rudat">
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

using System.IO.Abstractions;
using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Cpu;
using BOINC_To_MQTT.Gpu;
using BOINC_To_MQTT.Mqtt;
using BOINC_To_MQTT.Throttle;
using BOINC_To_MQTT.Work;
using Microsoft.Extensions.Options;

internal static class Program
{
    internal static void Main(string[] args) => CreateApplicationBuilder(args).Build().Run();

    internal static HostApplicationBuilder CreateApplicationBuilder(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services
            .AddSingleton<IFileSystem, FileSystem>()
            .AddSingleton(TimeProvider.System);

        builder.Services
            .AddSystemd()
            .AddWindowsService();

        builder.Services
            .AddSingleton<IValidateOptions<Boinc2MqttOptions>, ValidateBoinc2MqttOptions>();

        builder.Services
            .AddOptionsWithValidateOnStart<Boinc2MqttOptions>();

        builder.Services
            .Configure<Boinc2MqttOptions>(builder.Configuration.GetRequiredSection(Boinc2MqttOptions.ConfigurationSectionName));

        MqttConnection.Configure(builder);

        builder.Services
            .AddScoped<IBoincContext, BoincContext>();

        BoincConnection.Configure(builder);
        CpuController.Configure(builder);
        GpuController.Configure(builder);
        WorkController.Configure(builder);
        ThrottleController.Configure(builder);

        builder.Services
            .AddHostedService<BoincWorkerFactory>();

        return builder;
    }
}
