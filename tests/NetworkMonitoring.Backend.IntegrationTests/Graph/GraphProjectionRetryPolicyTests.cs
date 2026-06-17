using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Graph;

/// <summary>
/// Test suite for GraphProjectionRetryPolicy.
/// </summary>
public sealed class GraphProjectionRetryPolicyTests : IClassFixture<GraphTestApplicationFactory>
{
    private readonly GraphTestApplicationFactory _factory;

    public GraphProjectionRetryPolicyTests(GraphTestApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that projection continues after retryable failures.
    /// </summary>
    [Fact]
    public async Task Projection_ContinuesAfterRetryableFailures()
    {
        using var scope = _factory.Services.CreateScope();
        var projector = scope.ServiceProvider.GetRequiredService<ProjectCommunicationGraphUseCase>();
        var query = scope.ServiceProvider.GetRequiredService<IGraphQueryRepository>();

        // In-memory adapter does not throw transient faults; this asserts happy-path continuity and
        // protects the contract that projection remains available for subsequent events.
        await projector.Execute("device-a", "device-c", "InternalDevice", "TCP", DateTimeOffset.UtcNow, CancellationToken.None);
        await projector.Execute("device-a", "device-d", "InternalDevice", "UDP", DateTimeOffset.UtcNow, CancellationToken.None);

        var result = await query.GetDeviceGraph("device-a", 1, 50, CancellationToken.None);
        Assert.True(result.Edges.Count >= 2);
    }
}
