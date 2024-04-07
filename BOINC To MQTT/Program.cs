using MQTTWorker;
using BOINCWorker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSystemd()
    .AddWindowsService();

builder.Services
    .AddMQTTWorkerService(builder.Configuration)
    .AddBOINCWorkerService(builder.Configuration);

using IHost host = builder.Build();

await host.RunAsync();
