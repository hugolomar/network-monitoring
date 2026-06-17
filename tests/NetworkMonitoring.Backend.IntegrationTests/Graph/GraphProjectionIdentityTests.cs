using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Graph;

/// <summary>
/// Test suite for GraphProjectionIdentity.
/// </summary>
public sealed class GraphProjectionIdentityTests : IClassFixture<GraphTestApplicationFactory>
{
    private readonly GraphTestApplicationFactory _factory;

    public GraphProjectionIdentityTests(GraphTestApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that projection uses source destination protocol as identity.
    /// </summary>
    [Fact]
    public async Task Projection_UsesSourceDestinationProtocolAsIdentity()
    {
        using var scope = _factory.Services.CreateScope();
        var projector = scope.ServiceProvider.GetRequiredService<ProjectCommunicationGraphUseCase>();
        var query = scope.ServiceProvider.GetRequiredService<IGraphQueryRepository>();

        var when = DateTimeOffset.UtcNow;
        await projector.Execute("device-a", "device-b", "InternalDevice", "TCP", when, CancellationToken.None);
        await projector.Execute("device-a", "device-b", "InternalDevice", "UDP", when, CancellationToken.None);

        var result = await query.GetDeviceGraph("device-a", 1, 50, CancellationToken.None);
        Assert.Equal(2, result.Edges.Count);
        Assert.Contains(result.Edges, edge => edge.Protocol == "TCP");
        Assert.Contains(result.Edges, edge => edge.Protocol == "UDP");
    }
}
