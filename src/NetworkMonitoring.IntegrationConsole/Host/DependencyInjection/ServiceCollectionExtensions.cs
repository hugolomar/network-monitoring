using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetworkMonitoring.IntegrationConsole.Application.Configuration;
using NetworkMonitoring.IntegrationConsole.Application.Ports;
using NetworkMonitoring.IntegrationConsole.Application.UseCases;
using NetworkMonitoring.IntegrationConsole.Host.Services;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Backend;
using NetworkMonitoring.IntegrationConsole.Infrastructure.Ingestion;

namespace NetworkMonitoring.IntegrationConsole.Host.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationConsole(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<IntegrationConsoleOptions>()
            .Bind(configuration.GetSection(IntegrationConsoleOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.KafkaDeviceTopic), "Kafka device topic is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.BackendBaseUrl), "Backend base URL is required.")
            .ValidateOnStart();

        services.AddSingleton(sp => RetryOptions.From(sp.GetRequiredService<IOptions<IntegrationConsoleOptions>>().Value));
        services.AddSingleton<ProcessDeviceDetectionsUseCase>();
        services.AddSingleton<IDeviceEventConsumer, KafkaDeviceEventConsumer>();
        services.AddSingleton<DeviceIntakeRetryPolicy>();
        services.AddHttpClient<IDeviceIntakeClient, HttpDeviceIntakeClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<IntegrationConsoleOptions>>().Value;
            client.BaseAddress = new Uri(options.BackendBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.HttpTimeoutSeconds));
        });
        services.AddHostedService<IntegrationConsoleWorker>();

        return services;
    }
}
