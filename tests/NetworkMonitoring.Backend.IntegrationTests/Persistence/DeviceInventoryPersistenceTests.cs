using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Infrastructure.Persistence;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Persistence;

public sealed class DeviceInventoryPersistenceTests(BackendTestApplicationFactory factory) : IClassFixture<BackendTestApplicationFactory>
{
    [Fact]
    public async Task DbContext_model_defines_unique_mac_index()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DeviceInventoryDbContext>();

        var index = dbContext.Model.FindEntityType(typeof(DeviceInventoryRecord))!
            .GetIndexes()
            .Single(index => index.Properties.Any(property => property.Name == nameof(DeviceInventoryRecord.MacAddress)));

        Assert.True(index.IsUnique);
        Assert.True((await dbContext.Database.EnsureCreatedAsync()));
    }
}
