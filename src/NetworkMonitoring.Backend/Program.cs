using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Host.DependencyInjection;
using NetworkMonitoring.Backend.Host.Endpoints;
using NetworkMonitoring.Backend.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDeviceInventoryBackend(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.ApplyDeviceInventoryMigrations();
}

app.MapDeviceEndpoints();

app.Run();

public partial class Program;

internal static class DeviceInventoryMigrationExtensions
{
    public static async Task ApplyDeviceInventoryMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<BackendOptions>>().Value;
        if (!options.ApplyMigrationsOnStartup)
        {
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<DeviceInventoryDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
