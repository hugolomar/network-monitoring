using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Graph;

/// <summary>
/// Test suite for GraphProjectionIdempotency.
/// </summary>
public sealed class GraphProjectionIdempotencyTests : IClassFixture<GraphTestApplicationFactory>
{
    private readonly GraphTestApplicationFactory _factory;

    public GraphProjectionIdempotencyTests(GraphTestApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that projection replay does not duplicate structure.
    /// </summary>
    [Fact]
    public async Task Projection_ReplayDoesNotDuplicateStructure()
    {
        using var scope = _factory.Services.CreateScope();
        var projector = scope.ServiceProvider.GetRequiredService<ProjectCommunicationGraphUseCase>();
        var query = scope.ServiceProvider.GetRequiredService<IGraphQueryRepository>();

        var first = DateTimeOffset.UtcNow.AddMinutes(-1);
        var second = DateTimeOffset.UtcNow;
        await projector.Execute("device-a", "203.0.113.20", "ExternalHost", "TCP", first, CancellationToken.None);
        await projector.Execute("device-a", "203.0.113.20", "ExternalHost", "TCP", second, CancellationToken.None);

        var result = await query.GetDeviceGraph("device-a", 1, 50, CancellationToken.None);
        var edge = Assert.Single(result.Edges);
        Assert.Equal(2, edge.Weight);
        Assert.Equal(first, edge.FirstSeenUtc);
        Assert.Equal(second, edge.LastSeenUtc);
    }
}
