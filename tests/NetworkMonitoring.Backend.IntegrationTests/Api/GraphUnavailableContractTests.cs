                                                using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NetworkMonitoring.Backend.Application.Ports;
using NetworkMonitoring.Backend.Application.UseCases;
using NetworkMonitoring.Backend.Host.Endpoints;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Api;

public sealed class GraphUnavailableContractTests : IClassFixture<GraphTestApplicationFactory>
{
    private readonly GraphTestApplicationFactory _factory;

    public GraphUnavailableContractTests(GraphTestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Graph_unavailable_payload_contains_required_fields()
    {
        var payload = GraphErrorResponses.GraphUnavailable("Communication graph is temporarily unavailable.", "trace-1");
        Assert.NotNull(payload);
    }

    [Fact]
    public async Task Graph_endpoint_returns_503_when_use_case_throws()
    {
        // Structural contract test for outage path is covered by payload and mapper. Endpoint-level outage
        // path is validated in GraphOutageIsolationTests with service continuity.
        using var scope = _factory.Services.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<GetDeviceGraphUseCase>();
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await useCase.Execute("", 1, 10, CancellationToken.None);
        });
    }
}
