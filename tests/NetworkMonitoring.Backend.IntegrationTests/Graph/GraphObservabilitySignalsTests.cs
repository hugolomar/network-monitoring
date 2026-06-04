using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Graph;

/// <summary>
/// Test suite for GraphObservabilitySignals.
/// </summary>
public sealed class GraphObservabilitySignalsTests : IClassFixture<GraphTestApplicationFactory>
{
    private readonly GraphTestApplicationFactory _factory;

    public GraphObservabilitySignalsTests(GraphTestApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that projection and retention paths execute with telemetry bridge registered.
    /// </summary>
    [Fact]
    public async Task Projection_and_retention_paths_execute_with_telemetry_bridge_registered()
    {
        using var scope = _factory.Services.CreateScope();
        var projection = scope.ServiceProvider.GetRequiredService<ProjectCommunicationGraphUseCase>();
        var retention = scope.ServiceProvider.GetRequiredService<RunGraphRetentionSweepUseCase>();
        var telemetry = scope.ServiceProvider.GetRequiredService<IGraphTelemetry>();

        await projection.Execute("device-a", "device-z", "InternalDevice", "TCP", DateTimeOffset.UtcNow, CancellationToken.None);
        var outcome = await retention.Execute(CancellationToken.None);

        Assert.NotNull(telemetry);
        Assert.NotNull(outcome);
    }
}
