using System.Net;
using System.Net.Http.Json;
using NetworkMonitoring.Backend.Host.Endpoints;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Api;

/// <summary>
/// Test suite for GraphDevicesContract.
/// </summary>
public sealed class GraphDevicesContractTests(GraphTestApplicationFactory factory) : IClassFixture<GraphTestApplicationFactory>
{
    /// <summary>
    /// Verifies that get graph devices returns 401 when auth missing.
    /// </summary>
    [Fact]
    public async Task Get_graph_devices_returns_401_when_auth_missing()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/graph/devices?rootDeviceId=device-a");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Verifies that get graph devices returns contract shape for authorized role.
    /// </summary>
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
