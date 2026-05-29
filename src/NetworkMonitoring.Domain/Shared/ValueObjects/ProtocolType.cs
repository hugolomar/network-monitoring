namespace NetworkMonitoring.Domain.ValueObjects;

/// <summary>
/// Represents a validated network protocol (TCP, UDP, ICMP).
/// </summary>
public sealed class ProtocolType : ValueObject
{
    private static readonly HashSet<string> Allowed = ["TCP", "UDP", "ICMP", "OTHER"];

    /// <summary>
    /// Gets the normalized protocol name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolType"/> class.
    /// </summary>
    /// <param name="value">The raw protocol name.</param>
    public ProtocolType(string value)
    {
        Value = Normalize(value);
    }

    /// <summary>
    /// Creates a <see cref="ProtocolType"/> from a raw string or protocol number.
    /// </summary>
    /// <param name="raw">The raw input string.</param>
    /// <returns>A validated <see cref="ProtocolType"/> instance.</returns>
    public static ProtocolType FromRaw(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new ProtocolType("OTHER");
        }

        return int.TryParse(raw, out var protocolNumber)
            ? protocolNumber switch
            {
                6 => new ProtocolType("TCP"),
                17 => new ProtocolType("UDP"),
                1 => new ProtocolType("ICMP"),
                _ => new ProtocolType("OTHER")
            }
            : new ProtocolType(raw);
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
    /// Returns the string representation of the protocol.
    /// </summary>
    /// <returns>The normalized protocol name.</returns>
    public override string ToString() => Value;

    private static string Normalize(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        return Allowed.Contains(normalized) ? normalized : "OTHER";
    }
}
