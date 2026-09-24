using NetworkMonitoring.Backend.Host.Endpoints;

namespace NetworkMonitoring.Backend.UnitTests.Observability;

/// <summary>
/// Verifies baseline backend structured logging/error payload shape.
/// </summary>
public sealed class StructuredLoggingContractTests
{
    /// <summary>
    /// Ensures graph error payload includes mandatory context fields.
    /// </summary>
    [Fact]
    public void Graph_error_payload_contains_code_message_trace_and_timestamp()
    {
        var payload = GraphErrorResponses.GraphUnavailable("boom", "trace-1");
        var values = payload.GetType().GetProperties().ToDictionary(property => property.Name, property => property.GetValue(payload));

        Assert.Equal("GRAPH_UNAVAILABLE", values["code"]);
        Assert.Equal("boom", values["message"]);
        Assert.Equal("trace-1", values["traceId"]);
        Assert.NotNull(values["timestampUtc"]);
    }
}
