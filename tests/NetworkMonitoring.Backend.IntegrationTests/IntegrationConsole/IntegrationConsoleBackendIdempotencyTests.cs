using System.Net.Http.Json;
using NetworkMonitoring.Backend.IntegrationTests.Api;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.IntegrationConsole;

public sealed class IntegrationConsoleBackendIdempotencyTests(BackendTestApplicationFactory factory) : IClassFixture<BackendTestApplicationFactory>
{
    [Fact]
    public async Task Backend_keeps_forwarded_retries_idempotent()
    {
        var client = factory.CreateClient();

        await DeviceIntakeContractTests.PostValid(client);
        await DeviceIntakeContractTests.PostValid(client);

        var inventory = await client.GetFromJsonAsync<DeviceIntakeContractTests.InventoryResponse>("/devices");

        Assert.Single(inventory!.Items);
    }
}
