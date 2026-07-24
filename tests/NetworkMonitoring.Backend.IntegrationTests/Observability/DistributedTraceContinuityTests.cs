using Microsoft.AspNetCore.Mvc.Testing;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Observability;

/// <summary>
/// Verifies distributed trace continuity headers for backend endpoints.
/// </summary>
public sealed class DistributedTraceContinuityTests : IClassFixture<BackendTestApplicationFactory>
{
    private readonly BackendTestApplicationFactory _factory;

    /// <summary>
    /// Initializes test fixture.
    /// </summary>
    /// <param name="factory">Backend application factory.</param>
    public DistributedTraceContinuityTests(BackendTestApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Ensures response includes a W3C traceparent header.
    /// </summary>
    [Fact]
    public async Task Response_exposes_traceparent_header_for_request_trace_context()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync("/health/live");

        Assert.True(response.Headers.TryGetValues("traceparent", out var values));
        Assert.NotEmpty(values);
    }
}
