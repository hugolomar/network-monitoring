using System.Net;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Api;

/// <summary>
/// Test suite for GraphRetentionManualTriggerAbsence.
/// </summary>
public sealed class GraphRetentionManualTriggerAbsenceTests(GraphTestApplicationFactory factory) : IClassFixture<GraphTestApplicationFactory>
{
    /// <summary>
    /// Verifies that retention manual endpoint is not exposed.
    /// </summary>
    [Fact]
    public async Task Retention_manual_endpoint_is_not_exposed()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsync("/api/graph/retention/run", content: null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
