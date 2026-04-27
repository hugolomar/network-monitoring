using Avro.Generic;
using NetworkMonitoring.IntegrationConsole.Application.Models;
using System.Globalization;

namespace NetworkMonitoring.IntegrationConsole.Infrastructure.Serialization;

public static class DeviceDetectedEventMapper
{
    public static DeviceDetectedEvent FromGenericRecord(GenericRecord record)
    {
        return new DeviceDetectedEvent(
            GetRequiredString(record, "eventType"),
            GetDateTimeOffset(record, "occurredAtUtc"),
            GetRequiredString(record, "source"),
            GetRequiredInt(record, "schemaVersion"),
            GetNullableInt(record, "deviceId"),
            GetRequiredString(record, "macAddress"),
            GetNullableString(record, "primaryIp"),
            GetNullableString(record, "hostname"),
            GetStringArray(record, "observedIps"),
            GetDateTimeOffset(record, "firstSeenUtc"),
            GetDateTimeOffset(record, "lastSeenUtc"),
            GetRequiredString(record, "discoverySource"));
    }

    private static string GetRequiredString(GenericRecord record, string fieldName) =>
        GetNullableString(record, fieldName) ?? throw new InvalidOperationException($"Field '{fieldName}' is required.");

    private static string? GetNullableString(GenericRecord record, string fieldName) =>
        record.TryGetValue(fieldName, out var value) && value is not null ? value.ToString() : null;

    private static int GetRequiredInt(GenericRecord record, string fieldName) =>
        record.TryGetValue(fieldName, out var value) && value is int intValue
            ? intValue
            : throw new InvalidOperationException($"Field '{fieldName}' is required.");

    private static int? GetNullableInt(GenericRecord record, string fieldName) =>
        record.TryGetValue(fieldName, out var value) && value is not null ? Convert.ToInt32(value) : null;

    private static DateTimeOffset GetDateTimeOffset(GenericRecord record, string fieldName)
    {
        var value = GetRequiredString(record, fieldName);
        return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
    }

    private static IReadOnlyList<string> GetStringArray(GenericRecord record, string fieldName)
    {
        if (!record.TryGetValue(fieldName, out var value) || value is null)
        {
            return Array.Empty<string>();
        }

        return value switch
        {
            IEnumerable<string> strings => strings.ToArray(),
            IEnumerable<object> objects => objects.Select(item => item.ToString()).Where(item => item is not null).Cast<string>().ToArray(),
            _ => Array.Empty<string>()
        };
    }
}
