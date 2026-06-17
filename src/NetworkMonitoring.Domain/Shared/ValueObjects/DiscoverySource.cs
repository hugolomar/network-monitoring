namespace NetworkMonitoring.Domain.ValueObjects;

/// <summary>
/// Represents the source or method used for device discovery.
/// </summary>
public sealed class DiscoverySource : ValueObject
{
    private static readonly HashSet<string> Allowed = ["ARP", "LLDP", "CDP", "TRAFFIC", "OTHER"];

    /// <summary>
    /// Gets the normalized discovery source value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoverySource"/> class.
    /// </summary>
    /// <param name="value">The raw discovery source string.</param>
    public DiscoverySource(string value)
    {
        Value = Normalize(value);
    }

    /// <summary>
    /// Creates a <see cref="DiscoverySource"/> from a raw string, defaulting to TRAFFIC if empty.
    /// </summary>
    /// <param name="raw">The raw string input.</param>
    /// <returns>A validated <see cref="DiscoverySource"/> instance.</returns>
    public static DiscoverySource FromRaw(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new DiscoverySource("TRAFFIC");
        }

        return new DiscoverySource(raw);
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
    /// Returns the string representation of the discovery source.
    /// </summary>
    /// <returns>The normalized value.</returns>
    public override string ToString() => Value;

    private static string Normalize(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        return Allowed.Contains(normalized) ? normalized : "OTHER";
    }
}
