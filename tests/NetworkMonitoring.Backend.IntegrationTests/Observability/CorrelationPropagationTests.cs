using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.Observability;

/// <summary>
/// Validates backend correlation propagation headers.
/// </summary>
public sealed class CorrelationPropagationTests : IClassFixture<BackendTestApplicationFactory>
{
    private readonly BackendTestApplicationFactory _factory;

    /// <summary>
    /// Initializes test fixture.
    /// </summary>
    /// <param name="factory">Application factory for backend integration tests.</param>
    public CorrelationPropagationTests(BackendTestApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Ensures correlation header is reflected by middleware.
    /// </summary>
    [Fact]
    public async Task Correlation_header_is_reflected_in_response()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.TryAddWithoutValidation("X-Correlation-ID", "corr-test-123");

        var response = await client.SendAsync(request, CancellationToken.None);

        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        Assert.Contains("corr-test-123", values);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
