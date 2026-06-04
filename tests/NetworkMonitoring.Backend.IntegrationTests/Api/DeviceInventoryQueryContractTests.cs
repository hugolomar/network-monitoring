using System.Net;
using System.Net.Http.Json;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Api;

/// <summary>
/// Test suite for DeviceInventoryQueryContract.
/// </summary>
public sealed class DeviceInventoryQueryContractTests(BackendTestApplicationFactory factory) : IClassFixture<BackendTestApplicationFactory>
{
    /// <summary>
    /// Verifies that get devices returns empty inventory.
    /// </summary>
    [Fact]
    public async Task Get_devices_returns_empty_inventory()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/devices");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var inventory = await response.Content.ReadFromJsonAsync<DeviceIntakeContractTests.InventoryResponse>();
        Assert.Empty(inventory!.Items);
    }

    /// <summary>
    /// Verifies that get devices returns consolidated inventory.
    /// </summary>
    [Fact]
    public async Task Get_devices_returns_consolidated_inventory()
    {
        var client = factory.CreateClient();

        await DeviceIntakeContractTests.PostValid(client);

        var inventory = await client.GetFromJsonAsync<DeviceIntakeContractTests.InventoryResponse>("/devices");

        Assert.Single(inventory!.Items);
        Assert.Equal("AA:BB:CC:DD:EE:FF", inventory.Items[0].MacAddress);
    }
}
