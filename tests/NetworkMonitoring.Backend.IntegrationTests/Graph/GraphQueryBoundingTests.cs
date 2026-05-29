using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Graph;

public sealed class GraphQueryBoundingTests : IClassFixture<GraphTestApplicationFactory>
{
    private readonly GraphTestApplicationFactory _factory;

    public GraphQueryBoundingTests(GraphTestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Query_applies_depth_and_limit_caps()
    {
        using var scope = _factory.Services.CreateScope();
        var projector = scope.ServiceProvider.GetRequiredService<ProjectCommunicationGraphUseCase>();
        var queryUseCase = scope.ServiceProvider.GetRequiredService<GetDeviceGraphUseCase>();

        await projector.Execute("device-a", "device-b", "InternalDevice", "TCP", DateTimeOffset.UtcNow, CancellationToken.None);
        await projector.Execute("device-b", "device-c", "InternalDevice", "TCP", DateTimeOffset.UtcNow, CancellationToken.None);
        await projector.Execute("device-c", "device-d", "InternalDevice", "TCP", DateTimeOffset.UtcNow, CancellationToken.None);

        var result = await queryUseCase.Execute("device-a", depth: 99, limit: 1, CancellationToken.None);
        Assert.True(result.Truncated);
        Assert.Single(result.Nodes);
    }
}
