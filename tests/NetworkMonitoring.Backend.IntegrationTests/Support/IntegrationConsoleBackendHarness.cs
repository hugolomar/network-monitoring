namespace NetworkMonitoring.Backend.IntegrationTests.Support;

/// <summary>
/// Tests for IntegrationConsoleBackendHarness.
/// </summary>
public static class IntegrationConsoleBackendHarness
{
    public static IDictionary<string, string?> CreateConfiguration(Uri backendBaseAddress)
    {
        return new Dictionary<string, string?>
        {
            ["IntegrationConsole:BackendBaseUrl"] = backendBaseAddress.ToString()
        };
    }
}
