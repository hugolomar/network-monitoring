namespace NetworkMonitoring.Backend.IntegrationTests.Support;

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
