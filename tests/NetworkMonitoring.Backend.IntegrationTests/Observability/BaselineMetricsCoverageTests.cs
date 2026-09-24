using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Observability;

/// <summary>
/// Verifies baseline metrics-producing telemetry dependencies are registered.
/// </summary>
public sealed class BaselineMetricsCoverageTests : IClassFixture<GraphTestApplicationFactory>
{
    private readonly GraphTestApplicationFactory _factory;

    /// <summary>
    /// Initializes fixture instance.
    /// </summary>
    /// <param name="factory">Graph-capable backend test factory.</param>
    public BaselineMetricsCoverageTests(GraphTestApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Ensures graph projection path can execute with telemetry adapter enabled.
    /// </summary>
    [Fact]
    public async Task Graph_projection_path_runs_with_registered_metrics_adapter()
    {
        using var scope = _factory.Services.CreateScope();
        var projection = scope.ServiceProvider.GetRequiredService<ProjectCommunicationGraphUseCase>();
        var telemetry = scope.ServiceProvider.GetRequiredService<IGraphTelemetry>();

        await projection.Execute("device-a", "device-b", "InternalDevice", "UDP", DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.NotNull(telemetry);
    }
}
