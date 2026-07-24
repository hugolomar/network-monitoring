using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.Services;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.Infrastructure;
using NetworkMonitoring.Backend.Infrastructure.Graph;
using NetworkMonitoring.Backend.Infrastructure.Persistence;
using NetworkMonitoring.Backend.Host.Health;
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
            .AddBackendLogging(configuration)
            .AddBackendTelemetry(configuration);

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
        services.AddSingleton<IGraphTelemetry, NullGraphTelemetry>();
        services.AddSingleton<InMemoryGraphStore>();
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<BackendOptions>>().Value.Graph;
            return new Neo4jDriverAccessor(options);
        });

        services.AddScoped<Neo4jGraphProjectionRepository>();
        services.AddScoped<Neo4jGraphQueryRepository>();
        services.AddScoped<Neo4jGraphRetentionRepository>();

        services.AddScoped<IGraphProjectionRepository>(sp =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            var provider = sp.GetRequiredService<IOptions<BackendOptions>>().Value.Graph.Provider;
            if (env.IsEnvironment("Testing") || provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                return new InMemoryGraphProjectionRepository(sp.GetRequiredService<InMemoryGraphStore>());
            }

            return sp.GetRequiredService<Neo4jGraphProjectionRepository>();
        });

        services.AddScoped<IGraphQueryRepository>(sp =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            var provider = sp.GetRequiredService<IOptions<BackendOptions>>().Value.Graph.Provider;
            if (env.IsEnvironment("Testing") || provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                return new InMemoryGraphQueryRepository(sp.GetRequiredService<InMemoryGraphStore>());
            }

            return sp.GetRequiredService<Neo4jGraphQueryRepository>();
        });

        services.AddScoped<IGraphRetentionRepository>(sp =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            var provider = sp.GetRequiredService<IOptions<BackendOptions>>().Value.Graph.Provider;
            if (env.IsEnvironment("Testing") || provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                return new InMemoryGraphRetentionRepository(sp.GetRequiredService<InMemoryGraphStore>());
            }

            return sp.GetRequiredService<Neo4jGraphRetentionRepository>();
        });
        services.AddScoped<ProjectCommunicationGraphUseCase>();
        services.AddScoped<GetDeviceGraphUseCase>();
        services.AddScoped<RunGraphRetentionSweepUseCase>();
        services.AddSingleton<CriticalFlowObjectiveEvaluator>();
        services.AddHostedService<GraphRetentionHostedService>();
        services.AddHealthChecks()
            .AddCheck("backend-live", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
            .AddCheck<BackendReadinessHealthCheck>("backend-ready", tags: ["ready"]);

        return services;
    }
}
