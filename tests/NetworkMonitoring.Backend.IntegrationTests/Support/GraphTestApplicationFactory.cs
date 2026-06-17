namespace NetworkMonitoring.Backend.IntegrationTests.Support;

/// <summary>
/// Test host factory for graph-focused integration tests.
/// </summary>
public sealed class GraphTestApplicationFactory : IDisposable
{
    private readonly BackendTestApplicationFactory _inner = new();

    public HttpClient CreateClient()
    {
        return _inner.CreateClient();
    }

    public IServiceProvider Services => _inner.Services;

    public void Dispose()
    {
        _inner.Dispose();
    }
}
