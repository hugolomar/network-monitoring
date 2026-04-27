using System.Reflection;
using System.Text;
using Avro;
using Avro.Generic;
using NetworkMonitoring.Domain.Entities;

namespace NetworkMonitoring.Probe.Infrastructure.Publishing;

/// <summary>
/// Maps domain <see cref="Device"/> to Avro <see cref="GenericRecord"/> per embedded
/// <c>device-detected-value.avsc</c> (canonical copy of specs contract).
/// </summary>
public static class DeviceDetectedAvroMapper
{
    private static readonly RecordSchema DeviceValueSchema = LoadSchema();

    private static RecordSchema LoadSchema()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("device-detected-value.avsc")
            ?? throw new InvalidOperationException("Missing embedded resource device-detected-value.avsc.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return (RecordSchema)Schema.Parse(reader.ReadToEnd());
    }

    public static GenericRecord ToGenericRecord(Device device, DateTimeOffset occurredAtUtc)
    {
        var record = new GenericRecord(DeviceValueSchema);
        record.Add("eventType", "DeviceDetected");
        record.Add("occurredAtUtc", occurredAtUtc.ToUniversalTime().ToString("O"));
        record.Add("source", "probe");
        record.Add("schemaVersion", 1);
        record.Add("deviceId", device.Id.HasValue ? device.Id.Value : null);
        record.Add("macAddress", device.MacAddress.Value);
        record.Add("primaryIp", device.PrimaryIp?.Value);
        record.Add("hostname", device.Hostname);
        record.Add("observedIps", device.ObservedIps.Select(ip => ip.Value).Order(StringComparer.Ordinal).ToArray());
        record.Add("firstSeenUtc", device.FirstSeenUtc.ToUniversalTime().ToString("O"));
        record.Add("lastSeenUtc", device.LastSeenUtc.ToUniversalTime().ToString("O"));
        record.Add("discoverySource", device.DiscoverySource.Value);
        return record;
    }

    public static RecordSchema SchemaInstance => DeviceValueSchema;
}
