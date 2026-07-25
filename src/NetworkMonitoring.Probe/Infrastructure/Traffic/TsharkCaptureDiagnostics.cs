using System.Globalization;
using System.Text.RegularExpressions;

namespace NetworkMonitoring.Probe.Infrastructure.Traffic;

/// <summary>
/// Parses capture-loss signals from tshark/dumpcap stderr lines.
/// </summary>
public static partial class TsharkCaptureDiagnostics
{
    /// <summary>
    /// Attempts to read a dropped-packet count from a capture-process stderr line.
    /// </summary>
    /// <param name="line">A stderr line from tshark or dumpcap.</param>
    /// <param name="droppedCount">The parsed drop count when the line reports drops.</param>
    /// <returns><see langword="true"/> when the line reports one or more drops.</returns>
    public static bool TryReadCaptureDrops(string? line, out long droppedCount)
    {
        droppedCount = 0;
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var match = DroppedPacketsPattern().Match(line);
        if (!match.Success)
        {
            return false;
        }

        return long.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out droppedCount)
            && droppedCount > 0;
    }

    [GeneratedRegex(@"(\d+)\s+packets?\s+dropped", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DroppedPacketsPattern();
}
