using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.Infrastructure;
using NetworkMonitoring.Backend.Infrastructure.Persistence;

namespace NetworkMonitoring.Backend.Host.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeviceInventoryBackend(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<BackendOptions>()
            .Bind(configuration.GetSection(BackendOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Backend connection string is required.")
            .ValidateOnStart();

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

        return services;
    }
}
