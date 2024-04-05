using BOINC_To_MQTT;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSystemd()
    .AddWindowsService()
    .AddHostedService<MQTTWorker>()
    ;

//builder.Services.Add

using IHost host = builder.Build();

await host.RunAsync();
