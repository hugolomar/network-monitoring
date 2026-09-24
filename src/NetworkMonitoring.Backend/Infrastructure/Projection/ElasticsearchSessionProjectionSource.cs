using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NetworkMonitoring.Backend.Application.Configuration;
using NetworkMonitoring.Backend.Application.Models;
using NetworkMonitoring.Backend.Application.Ports;

namespace NetworkMonitoring.Backend.Infrastructure.Projection;

/// <summary>
/// Reads aggregated session records from Elasticsearch for graph projection sweeps.
/// </summary>
public sealed class ElasticsearchSessionProjectionSource(
    IHttpClientFactory httpClientFactory,
    IOptions<BackendOptions> options) : ISessionProjectionSource
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<SessionProjectionRecord>> GetAggregatedSessions(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken)
    {
        var projectionOptions = options.Value.Graph.ProjectionSource;
        var client = httpClientFactory.CreateClient("graph-projection-source");
        var request = BuildAggregationRequest(fromUtc, toUtc, projectionOptions.SweepBucketSize);
        var response = await client.PostAsJsonAsync(
            $"{projectionOptions.SessionsIndexName}/_search",
            request,
            SerializerOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var payload = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var records = new List<SessionProjectionRecord>();
        if (!payload.RootElement.TryGetProperty("aggregations", out var aggregations)
            || !aggregations.TryGetProperty("by_relationship", out var byRelationship)
            || !byRelationship.TryGetProperty("buckets", out var buckets)
            || buckets.ValueKind != JsonValueKind.Array)
        {
            return records;
        }

        foreach (var bucket in buckets.EnumerateArray())
        {
            var key = bucket.GetProperty("key");
            if (key.ValueKind != JsonValueKind.Array || key.GetArrayLength() < 3)
            {
                continue;
            }

            var sourceIp = key[0].GetString();
            var destinationIp = key[1].GetString();
            var protocol = key[2].GetString();
            if (string.IsNullOrWhiteSpace(sourceIp)
                || string.IsNullOrWhiteSpace(destinationIp)
                || string.IsNullOrWhiteSpace(protocol))
            {
                continue;
            }

            var count = TryReadLong(bucket, "doc_count");
            var firstSeen = TryReadDateTimeOffset(bucket, "first_seen", "value_as_string");
            var lastSeen = TryReadDateTimeOffset(bucket, "last_seen", "value_as_string");
            if (count <= 0 || firstSeen is null || lastSeen is null)
            {
                continue;
            }

            records.Add(new SessionProjectionRecord(
                sourceIp.Trim(),
                destinationIp.Trim(),
                protocol.Trim(),
                count,
                firstSeen.Value,
                lastSeen.Value));
        }

        return records;
    }

    private static Dictionary<string, object> BuildAggregationRequest(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int bucketSize)
    {
        var effectiveBucketSize = bucketSize <= 0 ? 1000 : bucketSize;
        return new Dictionary<string, object>
        {
            ["size"] = 0,
            ["query"] = new Dictionary<string, object>
            {
                ["bool"] = new Dictionary<string, object>
                {
                    ["must"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["range"] = new Dictionary<string, object>
                            {
                                ["lastSeenUtc"] = new Dictionary<string, object>
                                {
                                    ["gte"] = fromUtc.UtcDateTime.ToString("O"),
                                    ["lt"] = toUtc.UtcDateTime.ToString("O")
                                }
                            }
                        }
                    }
                }
            },
            ["aggs"] = new Dictionary<string, object>
            {
                ["by_relationship"] = new Dictionary<string, object>
                {
                    ["multi_terms"] = new Dictionary<string, object>
                    {
                        ["size"] = effectiveBucketSize,
                        // The session index template maps these as keyword directly, so there is
                        // no '.keyword' subfield to aggregate on; using one yields empty buckets
                        // instead of an error.
                        ["terms"] = new object[]
                        {
                            new Dictionary<string, object> { ["field"] = "sourceIp" },
                            new Dictionary<string, object> { ["field"] = "destinationIp" },
                            new Dictionary<string, object> { ["field"] = "protocol" }
                        }
                    },
                    ["aggs"] = new Dictionary<string, object>
                    {
                        ["first_seen"] = new Dictionary<string, object>
                        {
                            ["min"] = new Dictionary<string, object> { ["field"] = "firstSeenUtc" }
                        },
                        ["last_seen"] = new Dictionary<string, object>
                        {
                            ["max"] = new Dictionary<string, object> { ["field"] = "lastSeenUtc" }
                        }
                    }
                }
            }
        };
    }

    private static long TryReadLong(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return 0;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt64(out var number) => number,
            JsonValueKind.String when long.TryParse(value.GetString(), out var fromString) => fromString,
            _ => 0
        };
    }

    private static DateTimeOffset? TryReadDateTimeOffset(JsonElement root, string propertyName, string nestedPropertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property)
            || !property.TryGetProperty(nestedPropertyName, out var nested))
        {
            return null;
        }

        var raw = nested.GetString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return DateTimeOffset.TryParse(raw, out var timestamp) ? timestamp : null;
    }
}
