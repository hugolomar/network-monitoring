namespace NetworkMonitoring.Backend.Infrastructure.Graph;

/// <summary>
/// Translates infrastructure exceptions to graph endpoint error codes.
/// </summary>
public static class Neo4jGraphErrorMapper
{
    /// <summary>
    /// Maps dependency failures to stable graph error codes.
    /// </summary>
    public static string Map(Exception exception)
    {
        return exception switch
        {
            TimeoutException => "GRAPH_TIMEOUT",
            OperationCanceledException => "GRAPH_TIMEOUT",
            _ => "GRAPH_UNAVAILABLE"
        };
    }
}
