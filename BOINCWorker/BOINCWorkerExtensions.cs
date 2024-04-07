using CommonStuff;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BOINCWorker;

public static class BOINCWorkerExtensions
{
    public static IServiceCollection AddBOINCWorkerService(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services
            .AddHostedService<BOINCWorker>();

        services
            .AddSingleton<GetHostCrossProjectIdentifier>()
            .AddSingleton<CPUController>()
            .AddSingleton<GPUController>()
            .AddSingleton<ReadOldThrottleSetting>();

        services
            .Configure<BOINCWorkerOptions>(configuration.GetSection(BOINCWorkerOptions.ConfigurationSectionName));

        return services;
    }
}