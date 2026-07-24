using NetworkMonitoring.Backend.Host.Telemetry;

namespace NetworkMonitoring.Backend.IntegrationTests.Observability;

/// <summary>
/// Verifies Elasticsearch correlation query generation for diagnostics.
/// </summary>
public sealed class ElasticsearchLogCorrelationTests
{
    /// <summary>
    /// Ensures generated Elasticsearch query filters by correlation ID.
    /// </summary>
    [Fact]
    public void Correlation_query_contains_term_filter()
    {
        var query = ElasticsearchLogQueryBuilder.BuildCorrelationQuery("corr-55");

        Assert.True(query.ContainsKey("query"));
        var queryObject = Assert.IsType<Dictionary<string, object>>(query["query"]);
        var boolObject = Assert.IsType<Dictionary<string, object>>(queryObject["bool"]);
        var must = Assert.IsType<object[]>(boolObject["must"]);
        var term = Assert.IsType<Dictionary<string, object>>(must[0]);
        var termBody = Assert.IsType<Dictionary<string, object>>(term["term"]);
        Assert.Equal("corr-55", termBody["correlationId"]);
    }
}
