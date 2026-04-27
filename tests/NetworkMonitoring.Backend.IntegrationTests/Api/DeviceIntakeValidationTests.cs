using System.Net;
using System.Net.Http.Json;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Api;

public sealed class DeviceIntakeValidationTests(BackendTestApplicationFactory factory) : IClassFixture<BackendTestApplicationFactory>
{
    [Fact]
    public async Task Post_devices_rejects_missing_idempotency_key()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/devices", DeviceIntakeContractTests.ValidRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_devices_rejects_mismatched_mac_identity()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/devices")
        {
            Content = JsonContent.Create(DeviceIntakeContractTests.ValidRequest())
        };
        request.Headers.Add("Idempotency-Key", "11:22:33:44:55:66");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_devices_rejects_unsupported_content_type()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/devices")
        {
            Content = new StringContent("not-json")
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task Post_devices_rejects_invalid_timestamp_ordering()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/devices")
        {
            Content = JsonContent.Create(DeviceIntakeContractTests.ValidRequest(
                firstSeenUtc: "2026-04-27T12:05:00+00:00",
                lastSeenUtc: "2026-04-27T12:00:00+00:00"))
        };
        request.Headers.Add("Idempotency-Key", "AA:BB:CC:DD:EE:FF");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
