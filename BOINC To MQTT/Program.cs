using BOINC_To_MQTT;

internal class Program
{
    private static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services
            .AddSystemd()
            .AddWindowsService();

        builder.Services
            .AddHostedService<BOINC2MQTTWorker>()
            .AddHostedService<BOINCConnection>()
            .AddHostedService<MQTTConnection>();

        builder.Services
            .AddSingleton<CPUController>()
            .AddSingleton<GPUController>()
            .AddSingleton<ThrottleController>();

        builder.Services
            .Configure<BOINC2MQTTWorkerOptions>(builder.Configuration.GetSection(BOINC2MQTTWorkerOptions.ConfigurationSectionName));

        using IHost host = builder.Build();

        await host.RunAsync();
    }
}