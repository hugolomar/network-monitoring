using System.Reflection;
using System.Text;
using Avro;
using Avro.Generic;
using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

/// <summary>
/// Maps domain <see cref="Session"/> to Avro <see cref="GenericRecord"/> per embedded
/// <c>session-detected-value.avsc</c> (canonical copy of specs contract).
/// </summary>
public static class SessionDetectedAvroMapper
{
    private static readonly RecordSchema SessionValueSchema = LoadSchema();

    private static RecordSchema LoadSchema()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("session-detected-value.avsc")
            ?? throw new InvalidOperationException("Missing embedded resource session-detected-value.avsc.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return (RecordSchema)Schema.Parse(reader.ReadToEnd());
    }

    public static GenericRecord ToGenericRecord(Session session, DateTimeOffset occurredAtUtc)
    {
        var record = new GenericRecord(SessionValueSchema);
        record.Add("eventType", "SessionDetected");
        record.Add("occurredAtUtc", occurredAtUtc.ToUniversalTime().ToString("O"));
        record.Add("source", "probe");
        record.Add("schemaVersion", 1);
        record.Add("sessionId", session.Id.HasValue ? session.Id.Value : null);
        record.Add("sourceIp", session.SourceIp.Value);
        record.Add("destinationIp", session.DestinationIp.Value);
        record.Add("sourcePort", session.SourcePort?.Value);
        record.Add("destinationPort", session.DestinationPort?.Value);
        record.Add("protocol", session.Protocol.Value);
        record.Add("firstSeenUtc", session.FirstSeenUtc.ToUniversalTime().ToString("O"));
        record.Add("lastSeenUtc", session.LastSeenUtc.ToUniversalTime().ToString("O"));
        record.Add("bytesObserved", session.BytesObserved);
        return record;
    }

    public static RecordSchema SchemaInstance => SessionValueSchema;
}
