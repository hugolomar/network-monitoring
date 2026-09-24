namespace NetworkMonitoring.Backend.Host.Telemetry;

/// <summary>
/// Builds Elasticsearch log search payloads for correlation-based diagnostics.
/// </summary>
public static class ElasticsearchLogQueryBuilder
{
    /// <summary>
    /// Creates a baseline Elasticsearch query for a correlation identifier.
    /// </summary>
    /// <param name="correlationId">Correlation identifier to filter logs by.</param>
    /// <returns>Query payload represented as an anonymous-object-compatible dictionary.</returns>
    /// <exception cref="ArgumentException">Thrown when the correlation identifier is blank.</exception>
    public static IReadOnlyDictionary<string, object> BuildCorrelationQuery(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Correlation identifier is required.", nameof(correlationId));
        }

        return new Dictionary<string, object>
        {
            ["size"] = 200,
            ["sort"] = new object[]
            {
                new Dictionary<string, object>
                {
                    ["@timestamp"] = new Dictionary<string, object> { ["order"] = "desc" }
                }
            },
            ["query"] = new Dictionary<string, object>
            {
                ["bool"] = new Dictionary<string, object>
                {
                    ["must"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["term"] = new Dictionary<string, object> { ["correlationId"] = correlationId.Trim() }
                        }
                    }
                }
            }
        };
    }
}
