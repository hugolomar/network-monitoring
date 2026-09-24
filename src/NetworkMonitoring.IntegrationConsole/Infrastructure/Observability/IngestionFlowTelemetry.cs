using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using NetworkMonitoring.IntegrationConsole.Application.Ports;

namespace NetworkMonitoring.IntegrationConsole.Infrastructure.Observability;

/// <summary>
/// OpenTelemetry-backed ingestion flow metrics.
/// </summary>
public sealed class IngestionFlowTelemetry : IIngestionFlowTelemetry
{
    private static readonly Meter Meter = new("NetworkMonitoring.IntegrationConsole");
    private static readonly Counter<long> IngestionTotal = Meter.CreateCounter<long>("devices_ingested_total");
    private static readonly ConcurrentDictionary<(string Topic, int Partition), long> LagByPartition = new();

    // Synchronous Gauge measurements are not reliably exported via OTLP; observe last values on scrape.
    private static readonly ObservableGauge<long> ConsumerLag = Meter.CreateObservableGauge(
        "kafka_consumer_lag",
        ObserveConsumerLag);

    static IngestionFlowTelemetry()
    {
        // Touch the instrument so the type initializer always registers the observable callback.
        _ = ConsumerLag;
    }

    /// <inheritdoc />
    public void TrackIngestion(string outcome) =>
        IngestionTotal.Add(1, new KeyValuePair<string, object?>("outcome", outcome));

    /// <inheritdoc />
    public void TrackConsumerLag(string topic, int partition, long lag) =>
        LagByPartition[(topic, partition)] = Math.Max(0, lag);

    private static IEnumerable<Measurement<long>> ObserveConsumerLag()
    {
        foreach (var ((topic, partition), value) in LagByPartition)
        {
            yield return new Measurement<long>(
                value,
                new KeyValuePair<string, object?>("messaging.destination", topic),
                new KeyValuePair<string, object?>("messaging.kafka.partition", partition));
        }
    }
}
