using System.Text.RegularExpressions;

namespace NetworkMonitoring.Domain.ValueObjects;

/// <summary>
/// Represents a validated MAC address in canonical format (XX:XX:XX:XX:XX:XX).
/// </summary>
public sealed class MacAddress : ValueObject
{
    private static readonly Regex HexRegex = new("^[0-9A-F]{12}$", RegexOptions.Compiled);

    /// <summary>
    /// Gets the normalized MAC address string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacAddress"/> class.
    /// </summary>
    /// <param name="value">The raw MAC address string.</param>
    /// <exception cref="ArgumentException">Thrown when the MAC address format is invalid.</exception>
    public MacAddress(string value)
    {
        if (!TryNormalize(value, out var normalized))
        {
            throw new ArgumentException("Invalid MAC address.", nameof(value));
        }

        Value = normalized;
    }

    /// <summary>
    /// Tries to create a <see cref="MacAddress"/> instance from a string.
    /// </summary>
    /// <param name="value">The raw string.</param>
    /// <param name="macAddress">The resulting <see cref="MacAddress"/> or null if invalid.</param>
    /// <returns>True if creation was successful; otherwise, false.</returns>
    public static bool TryCreate(string? value, out MacAddress? macAddress)
    {
        macAddress = null;
        if (!TryNormalize(value, out _))
        {
            return false;
        }

        macAddress = new MacAddress(value!);
        return true;
    }

    /// <summary>
    /// Gets the components used for equality comparison.
    /// </summary>
    /// <returns>An enumeration of equality components.</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns the string representation of the MAC address.
    /// </summary>
    /// <returns>The normalized value.</returns>
    public override string ToString() => Value;

    private static bool TryNormalize(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var cleaned = value.Trim().Replace(":", "").Replace("-", "").ToUpperInvariant();
        if (!HexRegex.IsMatch(cleaned))
        {
            return false;
        }

        normalized = string.Join(":", Enumerable.Range(0, 6).Select(i => cleaned.Substring(i * 2, 2)));
        return true;
    }
}
