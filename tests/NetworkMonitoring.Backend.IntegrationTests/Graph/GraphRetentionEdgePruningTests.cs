using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Graph;

/// <summary>
/// Test suite for GraphRetentionEdgePruning.
/// </summary>
public sealed class GraphRetentionEdgePruningTests : IClassFixture<GraphTestApplicationFactory>
{
    private readonly GraphTestApplicationFactory _factory;

    public GraphRetentionEdgePruningTests(GraphTestApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that retention prunes relationships older than cutoff.
    /// </summary>
    [Fact]
    public async Task Retention_prunes_relationships_older_than_cutoff()
    {
        using var scope = _factory.Services.CreateScope();
        var projection = scope.ServiceProvider.GetRequiredService<IGraphProjectionRepository>();
        var retention = scope.ServiceProvider.GetRequiredService<RunGraphRetentionSweepUseCase>();
        var query = scope.ServiceProvider.GetRequiredService<IGraphQueryRepository>();

        await projection.UpsertCommunication(
            "device-a",
            "device-b",
            "InternalDevice",
            "TCP",
            DateTimeOffset.UtcNow.AddDays(-120),
            CancellationToken.None);

        await retention.Execute(CancellationToken.None);
        var result = await query.GetDeviceGraph("device-a", 1, 20, CancellationToken.None);
        Assert.Empty(result.Edges);
    }
}
