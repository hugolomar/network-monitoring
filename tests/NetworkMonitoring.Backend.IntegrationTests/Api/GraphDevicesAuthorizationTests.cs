using System.Net;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Api;

public sealed class GraphDevicesAuthorizationTests(GraphTestApplicationFactory factory) : IClassFixture<GraphTestApplicationFactory>
{
    [Fact]
    public async Task Get_graph_devices_returns_403_for_role_outside_allowed_set()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        client.DefaultRequestHeaders.Add("X-Role", "guest");

        var response = await client.GetAsync("/api/graph/devices?rootDeviceId=device-a");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
