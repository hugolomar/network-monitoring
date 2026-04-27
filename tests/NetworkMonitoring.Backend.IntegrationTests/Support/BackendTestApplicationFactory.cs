using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Infrastructure.Persistence;

namespace NetworkMonitoring.Backend.IntegrationTests.Support;

public sealed class BackendTestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"device-inventory-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(descriptor => descriptor.ServiceType == typeof(DbContextOptions<DeviceInventoryDbContext>)
                    || descriptor.ServiceType == typeof(DeviceInventoryDbContext)
                    || descriptor.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration", StringComparison.Ordinal) is true)
                .ToArray();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<DeviceInventoryDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
