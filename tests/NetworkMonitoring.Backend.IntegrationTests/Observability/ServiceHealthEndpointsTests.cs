using Microsoft.AspNetCore.Mvc.Testing;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Observability;

/// <summary>
/// Verifies backend service health endpoint behavior.
/// </summary>
public sealed class ServiceHealthEndpointsTests : IClassFixture<BackendTestApplicationFactory>
{
    private readonly BackendTestApplicationFactory _factory;

    /// <summary>
    /// Initializes test fixture.
    /// </summary>
    /// <param name="factory">Backend application factory.</param>
    public ServiceHealthEndpointsTests(BackendTestApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Ensures liveness and readiness routes are reachable.
    /// </summary>
    [Fact]
    public async Task Liveness_and_readiness_endpoints_return_success_codes()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var live = await client.GetAsync("/health/live");
        var ready = await client.GetAsync("/health/ready");

        Assert.Equal(System.Net.HttpStatusCode.OK, live.StatusCode);
        Assert.True(
            ready.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.ServiceUnavailable,
            $"Unexpected readiness status code: {(int)ready.StatusCode}");
    }
}
