using System.Net.Http.Json;
using NetworkMonitoring.Backend.IntegrationTests.Api;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Persistence;

public sealed class DeviceInventoryUniquenessTests(BackendTestApplicationFactory factory) : IClassFixture<BackendTestApplicationFactory>
{
    [Fact]
    public async Task Repeated_intake_keeps_single_inventory_item_for_mac()
    {
        var client = factory.CreateClient();

        await DeviceIntakeContractTests.PostValid(client);
        await DeviceIntakeContractTests.PostValid(client);

        var inventory = await client.GetFromJsonAsync<DeviceIntakeContractTests.InventoryResponse>("/devices");

        Assert.Single(inventory!.Items);
    }
}
