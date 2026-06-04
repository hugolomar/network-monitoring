using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Graph;

/// <summary>
/// Test suite for GraphRetentionNodeCleanup.
/// </summary>
public sealed class GraphRetentionNodeCleanupTests : IClassFixture<GraphTestApplicationFactory>
{
    private readonly GraphTestApplicationFactory _factory;

    public GraphRetentionNodeCleanupTests(GraphTestApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that retention removes orphan external hosts but not internal devices.
    /// </summary>
    [Fact]
    public async Task Retention_removes_orphan_external_hosts_but_not_internal_devices()
    {
        using var scope = _factory.Services.CreateScope();
        var projection = scope.ServiceProvider.GetRequiredService<IGraphProjectionRepository>();
        var retention = scope.ServiceProvider.GetRequiredService<RunGraphRetentionSweepUseCase>();
        var query = scope.ServiceProvider.GetRequiredService<IGraphQueryRepository>();

        await projection.UpsertCommunication(
            "device-a",
            "203.0.113.20",
            "ExternalHost",
            "TCP",
            DateTimeOffset.UtcNow.AddDays(-120),
            CancellationToken.None);

        await retention.Execute(CancellationToken.None);
        var result = await query.GetDeviceGraph("device-a", 1, 20, CancellationToken.None);
        Assert.Contains(result.Nodes, node => node.Id == "device-a");
        Assert.DoesNotContain(result.Nodes, node => node.Id == "203.0.113.20");
    }
}
