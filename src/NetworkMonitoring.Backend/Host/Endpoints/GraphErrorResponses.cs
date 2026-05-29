namespace NetworkMonitoring.Backend.Host.Endpoints;

/// <summary>
/// Helpers to build stable graph error response payloads.
/// </summary>
public static class GraphErrorResponses
{
    /// <summary>
    /// Creates a graph invalid-request payload.
    /// </summary>
    public static object InvalidRequest(string message, string? traceId = null)
    {
        return new
        {
            code = "GRAPH_INVALID_REQUEST",
            message,
            traceId = traceId ?? string.Empty,
            timestampUtc = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a graph unavailable payload.
    /// </summary>
    public static object GraphUnavailable(string message, string? traceId = null)
    {
        return new
        {
            code = "GRAPH_UNAVAILABLE",
            message,
            traceId = traceId ?? string.Empty,
            timestampUtc = DateTimeOffset.UtcNow
        };
    }
}
