using System.Text.RegularExpressions;

namespace NetworkMonitoring.Backend.Host.Telemetry;

/// <summary>
/// Redacts sensitive fragments from telemetry text payloads.
/// </summary>
public static partial class TelemetryRedaction
{
    /// <summary>
    /// Redacts credential-like and secret-like tokens in a message.
    /// </summary>
    /// <param name="message">Original message.</param>
    /// <returns>Redacted message safe for telemetry output.</returns>
    public static string Redact(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        var sanitized = SecretPattern().Replace(message, "***");
        sanitized = PasswordPattern().Replace(sanitized, "$1=***");
        return sanitized;
    }

    /// <summary>
    /// Redacts all string values in a structured property bag.
    /// </summary>
    /// <param name="properties">Original telemetry properties.</param>
    /// <returns>Redacted telemetry properties.</returns>
    public static IReadOnlyDictionary<string, object?> RedactProperties(IReadOnlyDictionary<string, object?> properties)
    {
        var sanitized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in properties)
        {
            sanitized[key] = value is string text ? Redact(text) : value;
        }

        return sanitized;
    }

    [GeneratedRegex(@"(?i)(api[_-]?key|token|secret)\s*[:=]\s*[^\s,;]+")]
    private static partial Regex SecretPattern();

    [GeneratedRegex(@"(?i)\b(password)\s*[:=]\s*[^\s,;]+")]
    private static partial Regex PasswordPattern();
}
