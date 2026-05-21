using System.Text;
using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

/// <summary>
/// Kafka message key: same deterministic identity as session deduplication (spec FR-014 / ProcessObservationsUseCase fingerprint).
/// </summary>
public static class SessionKafkaPartitionKey
{
    /// <summary>
    /// Builds a deterministic string key for Kafka partitioning based on the session fingerprint (IPs, Ports, Protocol).
    /// </summary>
    /// <param name="session">The session to build a key for.</param>
    /// <returns>A string representation of the partition key.</returns>
    public static string Build(Session session) =>
        $"{session.SourceIp.Value}|{session.DestinationIp.Value}|{session.SourcePort?.Value}|{session.DestinationPort?.Value}|{session.Protocol.Value}";

    /// <summary>
    /// Builds a deterministic byte-array key for Kafka partitioning.
    /// </summary>
    /// <param name="session">The session to build a key for.</param>
    /// <returns>The key encoded as UTF-8 bytes.</returns>
    public static byte[] BuildUtf8Bytes(Session session) => Encoding.UTF8.GetBytes(Build(session));
}
