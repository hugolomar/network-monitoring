using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Host.DependencyInjection;
using NetworkMonitoring.Backend.Host.Endpoints;
using NetworkMonitoring.Backend.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDeviceInventoryBackend(builder.Configuration);

var app = builder.Build();

// Skip database migrations during integration tests
if (!app.Environment.IsEnvironment("Testing"))
{
    await app.ApplyDeviceInventoryMigrations();
}

app.MapDeviceEndpoints();
app.MapGraphEndpoints();

app.Run();

namespace NetworkMonitoring.Backend
{
    /// <summary>
    /// Entry point for the Network Monitoring Backend application.
    /// </summary>
    public partial class Program { }
}

/// <summary>
/// Extension methods for database migration on startup.
/// </summary>
internal static class DeviceInventoryMigrationExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations to the device inventory database.
    /// </summary>
    /// <param name="app">The web application host.</param>
    public static async Task ApplyDeviceInventoryMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<BackendOptions>>().Value;
        
        // Only apply migrations if explicitly configured in appsettings
        if (!options.ApplyMigrationsOnStartup)
        {
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<DeviceInventoryDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
