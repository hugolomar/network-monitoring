using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Graph;

/// <summary>
/// Integration tests for projection sweeps sourced from indexed sessions.
/// </summary>
public sealed class GraphProjectionSweepTests : IClassFixture<GraphTestApplicationFactory>
{
    private readonly GraphTestApplicationFactory _factory;

    public GraphProjectionSweepTests(GraphTestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SweepProjection_MapsInventoryIpAndProjectsRelationships()
    {
        using var scope = _factory.Services.CreateScope();
        var intake = scope.ServiceProvider.GetRequiredService<AcceptDeviceIntakeUseCase>();
        var projector = scope.ServiceProvider.GetRequiredService<ProjectCommunicationGraphUseCase>();
        var query = scope.ServiceProvider.GetRequiredService<IGraphQueryRepository>();

        var now = DateTimeOffset.UtcNow;
        await intake.Execute(
            new DeviceIntakeCommand(
                "aa:bb:cc:dd:ee:01",
                "aa:bb:cc:dd:ee:01",
                "10.0.0.10",
                "sensor-a",
                ["10.0.0.10"],
                now.AddMinutes(-10),
                now.AddMinutes(-1),
                "probe",
                null),
            CancellationToken.None);
        await intake.Execute(
            new DeviceIntakeCommand(
                "aa:bb:cc:dd:ee:02",
                "aa:bb:cc:dd:ee:02",
                "10.0.0.20",
                "sensor-b",
                ["10.0.0.20"],
                now.AddMinutes(-10),
                now.AddMinutes(-1),
                "probe",
                null),
            CancellationToken.None);

        var records = new[]
        {
            new SessionProjectionRecord(
                "10.0.0.10",
                "10.0.0.20",
                "TCP",
                12,
                now.AddMinutes(-20),
                now.AddMinutes(-2))
        };

        await projector.ExecuteSweep(records, CancellationToken.None);

        var graph = await query.GetDeviceGraph("device-1", 1, 20, CancellationToken.None);
        Assert.Contains(graph.Nodes, node => node.Id == "device-1" && node.Kind == "InternalDevice");
        Assert.Contains(graph.Nodes, node => node.Id == "device-2" && node.Kind == "InternalDevice");
        Assert.Contains(graph.Edges, edge =>
            edge.SourceId == "device-1"
            && edge.DestinationId == "device-2"
            && edge.Protocol == "TCP"
            && edge.Weight == 12);
    }
}
