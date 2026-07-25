namespace NetworkMonitoring.IntegrationConsole.Application.Ports;

/// <summary>
/// Emits ingestion throughput and consumer lag for the integration console (FR-015, FR-017).
/// </summary>
public interface IIngestionFlowTelemetry
{
    /// <summary>
    /// Records one processed device-detection event.
    /// </summary>
    /// <param name="outcome">Outcome kind string (for example succeeded, rejected).</param>
    void TrackIngestion(string outcome);

    /// <summary>
    /// Records the current consumer lag for a topic partition.
    /// </summary>
    /// <param name="topic">Kafka topic.</param>
    /// <param name="partition">Partition id.</param>
    /// <param name="lag">Messages remaining behind the high watermark.</param>
    void TrackConsumerLag(string topic, int partition, long lag);
}
