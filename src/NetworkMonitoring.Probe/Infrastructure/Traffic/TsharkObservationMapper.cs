using NetworkMonitoring.Probe.Application.Models;

namespace NetworkMonitoring.Probe.Infrastructure.Traffic;

/// <summary>
/// Handles the transformation of raw text lines from tshark output into <see cref="TrafficObservation"/> domain models.
/// </summary>
public sealed class TsharkObservationMapper
{
    /// <summary>
    /// Attempts to parse a tab-separated line from tshark into a structured observation.
    /// </summary>
    /// <param name="line">The raw text line produced by the tshark process (expected to be tab-separated).</param>
    /// <param name="observation">When this method returns, contains the parsed <see cref="TrafficObservation"/> if successful; otherwise, null.</param>
    /// <returns>True if the line was successfully parsed; otherwise, false.</returns>
    /// <remarks>
    /// Parsing logic:
    /// - Expects at least 9 fields separated by tabs as configured in the provider's arguments.
    /// - Source and Destination IPs (fields 0 and 1) are mandatory.
    /// - Handles optional ports and epoch-based timestamps.
    /// </remarks>
    public bool TryMap(string line, out TrafficObservation? observation)
    {
        observation = null;
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        // Tshark is configured to output fields separated by tabs (\t).
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

    /// <summary>
    /// Parses a tshark timestamp (typically Unix epoch format).
    /// </summary>
    private static DateTimeOffset ParseObservedAt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DateTimeOffset.UtcNow;
        }

        // Tshark often outputs seconds since epoch with millisecond precision (e.g., 1621584000.123).
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
