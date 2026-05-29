using System.Net;
using System.Net.Http.Json;
using NetworkMonitoring.Backend.Host.Endpoints;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Api;

public sealed class GraphDevicesContractTests(GraphTestApplicationFactory factory) : IClassFixture<GraphTestApplicationFactory>
{
    [Fact]
    public async Task Get_graph_devices_returns_401_when_auth_missing()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/graph/devices?rootDeviceId=device-a");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_graph_devices_returns_contract_shape_for_authorized_role()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        client.DefaultRequestHeaders.Add("X-Role", "analyst");

        var response = await client.GetAsync("/api/graph/devices?rootDeviceId=device-a&depth=1&limit=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<GraphDevicesResponseDto>();
        Assert.NotNull(payload);
        Assert.NotNull(payload!.Nodes);
        Assert.NotNull(payload.Edges);
    }
}
