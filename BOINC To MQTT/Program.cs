using BOINC_To_MQTT.Boinc;
using BOINC_To_MQTT.Cpu;
using BOINC_To_MQTT.Gpu;
using BOINC_To_MQTT.Mqtt;
using BOINC_To_MQTT.Throttle;
using BOINC_To_MQTT.Work;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace BOINC_To_MQTT;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = CreateApplicationBuilder(args);

        using var host = builder.Build();

        host.Run();
    }

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
