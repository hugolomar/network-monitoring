using System.Text;
using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

/// <summary>
/// Kafka message key: same deterministic identity as session deduplication (spec FR-014 / ProcessObservationsUseCase fingerprint).
/// </summary>
public static class SessionKafkaPartitionKey
{
    public static string Build(Session session) =>
        $"{session.SourceIp.Value}|{session.DestinationIp.Value}|{session.SourcePort?.Value}|{session.DestinationPort?.Value}|{session.Protocol.Value}";

    public static byte[] BuildUtf8Bytes(Session session) => Encoding.UTF8.GetBytes(Build(session));
}
