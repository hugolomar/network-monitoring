using System.Net;

namespace NetworkMonitoring.Domain.ValueObjects;

/// <summary>
/// Represents a validated IP address (IPv4 or IPv6).
/// </summary>
public sealed class IpAddress : ValueObject
{
    /// <summary>
    /// Gets the string representation of the IP address.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IpAddress"/> class.
    /// </summary>
    /// <param name="value">The raw IP address string.</param>
    /// <exception cref="ArgumentException">Thrown when the IP address format is invalid.</exception>
    public IpAddress(string value)
    {
        if (!TryNormalize(value, out var normalized))
        {
            throw new ArgumentException("Invalid IP address.", nameof(value));
        }

        Value = normalized;
    }

    /// <summary>
    /// Tries to create an <see cref="IpAddress"/> instance from a string.
    /// </summary>
    /// <param name="value">The raw string.</param>
    /// <param name="ipAddress">The resulting <see cref="IpAddress"/> or null if invalid.</param>
    /// <returns>True if creation was successful; otherwise, false.</returns>
    public static bool TryCreate(string? value, out IpAddress? ipAddress)
    {
        ipAddress = null;
        if (!TryNormalize(value, out _))
        {
            return false;
        }

        ipAddress = new IpAddress(value!);
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
    /// Returns the string representation of the IP address.
    /// </summary>
    /// <returns>The IP address value.</returns>
    public override string ToString() => Value;

    private static bool TryNormalize(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!IPAddress.TryParse(value.Trim(), out var ip))
        {
            return false;
        }

        normalized = ip.ToString();
        return true;
    }
}
