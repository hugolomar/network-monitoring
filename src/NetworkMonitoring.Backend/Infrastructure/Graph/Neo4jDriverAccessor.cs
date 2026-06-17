using Neo4j.Driver;
using NetworkMonitoring.Backend.Application.Configuration;

namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// Lazily creates and shares one Neo4j driver instance.
/// </summary>
public sealed class Neo4jDriverAccessor : IAsyncDisposable
{
    private readonly GraphOptions _options;
    private IDriver? _driver;

    /// <summary>
    /// Initializes a new accessor with graph options.
    /// </summary>
    public Neo4jDriverAccessor(GraphOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Gets the shared Neo4j driver instance, creating it on first use.
    /// </summary>
    public IDriver GetOrCreate()
    {
        _driver ??= GraphDatabase.Driver(
            _options.Neo4jUri,
            AuthTokens.Basic(_options.Neo4jUsername, _options.Neo4jPassword));
        return _driver;
    }

    /// <summary>
    /// Disposes the underlying Neo4j driver when available.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_driver is not null)
        {
            await _driver.DisposeAsync();
        }
    }
}

