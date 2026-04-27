using System.Net;
using System.Net.Http.Json;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Api;

public sealed class DeviceIntakeContractTests(BackendTestApplicationFactory factory) : IClassFixture<BackendTestApplicationFactory>
{
    [Fact]
    public async Task Post_devices_accepts_valid_request()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/devices")
        {
            Content = JsonContent.Create(ValidRequest())
        };
        request.Headers.Add("Idempotency-Key", "AA:BB:CC:DD:EE:FF");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var inventory = await client.GetFromJsonAsync<InventoryResponse>("/devices");
        Assert.Single(inventory!.Items);
        Assert.Equal("AA:BB:CC:DD:EE:FF", inventory.Items[0].MacAddress);
    }

    [Fact]
    public async Task Post_devices_treats_duplicate_as_success_without_duplicate_inventory_item()
    {
        var client = factory.CreateClient();

        await PostValid(client);
        var duplicate = await PostValid(client);

        Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);
        var inventory = await client.GetFromJsonAsync<InventoryResponse>("/devices");
        Assert.Single(inventory!.Items);
    }

    internal static object ValidRequest(
        string macAddress = "AA:BB:CC:DD:EE:FF",
        string? primaryIp = "192.168.1.10",
        string? hostname = "switch-01",
        string[]? observedIps = null,
        string firstSeenUtc = "2026-04-27T12:00:00+00:00",
        string lastSeenUtc = "2026-04-27T12:05:00+00:00")
    {
        return new
        {
            macAddress,
            primaryIp,
            hostname,
            observedIps = observedIps ?? ["192.168.1.10"],
            firstSeenUtc,
            lastSeenUtc,
            discoverySource = "TRAFFIC",
            sourceEvent = new
            {
                eventType = "DeviceDetected",
                source = "probe",
                schemaVersion = 1,
                occurredAtUtc = "2026-04-27T12:05:01+00:00"
            }
        };
    }

    internal static async Task<HttpResponseMessage> PostValid(HttpClient client)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/devices")
        {
            Content = JsonContent.Create(ValidRequest())
        };
        request.Headers.Add("Idempotency-Key", "AA:BB:CC:DD:EE:FF");
        return await client.SendAsync(request);
    }

    internal sealed record InventoryResponse(DeviceItem[] Items);

    internal sealed record DeviceItem(
        int Id,
        string MacAddress,
        string? PrimaryIp,
        string? Hostname,
        string[] ObservedIps,
        DateTimeOffset FirstSeenUtc,
        DateTimeOffset LastSeenUtc,
        string DiscoverySource);
}
