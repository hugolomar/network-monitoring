using System.Net;
using NetworkMonitoring.Backend.IntegrationTests.Api;
using NetworkMonitoring.Backend.IntegrationTests.Support;

namespace NetworkMonitoring.Backend.IntegrationTests.IntegrationConsole;

/// <summary>
/// Test suite for IntegrationConsoleForwardingToBackend.
/// </summary>
public sealed class IntegrationConsoleForwardingToBackendTests(BackendTestApplicationFactory factory) : IClassFixture<BackendTestApplicationFactory>
{
    /// <summary>
    /// Verifies that backend accepts the http contract forwarded by integration console.
    /// </summary>
    [Fact]
    public async Task Backend_accepts_the_http_contract_forwarded_by_integration_console()
    {
        var client = factory.CreateClient();

        var response = await DeviceIntakeContractTests.PostValid(client);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
