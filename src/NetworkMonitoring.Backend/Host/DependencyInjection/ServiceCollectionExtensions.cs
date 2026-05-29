using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.Infrastructure;
using NetworkMonitoring.Backend.Infrastructure.Graph;
using NetworkMonitoring.Backend.Infrastructure.Persistence;
using NetworkMonitoring.Backend.Host.Services;

namespace NetworkMonitoring.Backend.Host.DependencyInjection;

/// <summary>
/// Extension methods for setting up backend services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the device inventory backend services, including persistence and use cases.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration to bind settings from.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddDeviceInventoryBackend(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<BackendOptions>()
            .Bind(configuration.GetSection(BackendOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Backend connection string is required.")
            .ValidateOnStart();

        // Configure Entity Framework Core with PostgreSQL using the connection string from configuration
        services.AddDbContext<DeviceInventoryDbContext>((sp, options) =>
        {
            var backendOptions = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
            options.UseNpgsql(backendOptions.ConnectionString);
        });

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<EfDeviceInventoryRepository>();
        services.AddScoped<IDeviceInventoryRepository>(sp => sp.GetRequiredService<EfDeviceInventoryRepository>());
        services.AddScoped<IInventoryUnitOfWork>(sp => sp.GetRequiredService<EfDeviceInventoryRepository>());
        services.AddScoped<AcceptDeviceIntakeUseCase>();
        services.AddScoped<ListDevicesUseCase>();
        services.AddSingleton<InMemoryGraphStore>();
        services.AddSingleton<IGraphTelemetry, NullGraphTelemetry>();
        services.AddScoped<IGraphProjectionRepository, Neo4jGraphProjectionRepository>();
        services.AddScoped<IGraphQueryRepository, Neo4jGraphQueryRepository>();
        services.AddScoped<IGraphRetentionRepository, Neo4jGraphRetentionRepository>();
        services.AddScoped<ProjectCommunicationGraphUseCase>();
        services.AddScoped<GetDeviceGraphUseCase>();
        services.AddScoped<RunGraphRetentionSweepUseCase>();
        services.AddHostedService<GraphRetentionHostedService>();

        return services;
    }
}
