using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MQTTWorker;

public static class MQTTWorkerExtensions
{
    public static IServiceCollection AddMQTTWorkerService(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services
            .AddHostedService<MQTTWorker>();

        services
            .Configure<MQTTWorkerOptions>(configuration.GetSection(MQTTWorkerOptions.ConfigurationSectionName));

        return services;
    }

}