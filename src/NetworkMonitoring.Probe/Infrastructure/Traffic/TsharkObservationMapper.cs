using NetworkMonitoring.Probe.Application.Models;

namespace NetworkMonitoring.Probe.Infrastructure.Traffic;

public sealed class TsharkObservationMapper
{
    public bool TryMap(string line, out TrafficObservation? observation)
    {
        observation = null;
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var parts = line.Split('\t');
        if (parts.Length < 9)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            return false;
        }

        var observedAt = ParseObservedAt(parts[5]);
        var bytesObserved = ParseBytes(parts[6]);

        observation = new TrafficObservation(
            parts[0],
            parts[1],
            ParseNullablePort(parts[2]),
            ParseNullablePort(parts[3]),
            parts[4],
            observedAt,
            bytesObserved,
            NullIfEmpty(parts.ElementAtOrDefault(7)),
            NullIfEmpty(parts.ElementAtOrDefault(8)),
            NullIfEmpty(parts.ElementAtOrDefault(9)),
            "TRAFFIC");

        return true;
    }

    private static DateTimeOffset ParseObservedAt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DateTimeOffset.UtcNow;
        }

        if (double.TryParse(value, out var epoch))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds((long)(epoch * 1000));
        }

        return DateTimeOffset.TryParse(value, out var parsed)
            ? parsed
            : DateTimeOffset.UtcNow;
    }

    private static long ParseBytes(string? value)
    {
        return long.TryParse(value, out var bytesObserved) && bytesObserved >= 0
            ? bytesObserved
            : 0;
    }

    private static int? ParseNullablePort(string? value)
    {
        if (int.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
