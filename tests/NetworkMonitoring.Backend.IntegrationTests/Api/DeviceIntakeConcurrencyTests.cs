using System.Net;
using System.Net.Http.Json;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Api;

public sealed class DeviceIntakeConcurrencyTests(BackendTestApplicationFactory factory) : IClassFixture<BackendTestApplicationFactory>
{
    [Fact]
    public async Task Concurrent_duplicate_intake_keeps_single_inventory_item()
    {
        var client = factory.CreateClient();

        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => PostValid(client)));

        Assert.All(responses, response => Assert.True(
            response.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK,
            $"Unexpected status code: {response.StatusCode}"));

        var inventory = await client.GetFromJsonAsync<DeviceIntakeContractTests.InventoryResponse>("/devices");
        Assert.Single(inventory!.Items);
    }

    private static async Task<HttpResponseMessage> PostValid(HttpClient client)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/devices")
        {
            Content = JsonContent.Create(DeviceIntakeContractTests.ValidRequest())
        };
        request.Headers.Add("Idempotency-Key", "AA:BB:CC:DD:EE:FF");
        return await client.SendAsync(request);
    }
}
