using System.Net;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Api;

public sealed class GraphOutageIsolationTests(GraphTestApplicationFactory factory) : IClassFixture<GraphTestApplicationFactory>
{
    [Fact]
    public async Task Device_endpoints_remain_available_when_graph_calls_fail()
    {
        var client = factory.CreateClient();
        var devicesResponse = await client.GetAsync("/devices");
        Assert.Equal(HttpStatusCode.OK, devicesResponse.StatusCode);
    }
}
