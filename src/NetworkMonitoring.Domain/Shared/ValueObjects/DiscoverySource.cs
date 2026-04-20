namespace NetworkMonitoring.Domain.ValueObjects;

public sealed class DiscoverySource : ValueObject
{
    private static readonly HashSet<string> Allowed = ["ARP", "LLDP", "CDP", "TRAFFIC", "OTHER"];

    public string Value { get; }

    public DiscoverySource(string value)
    {
        Value = Normalize(value);
    }

    public static DiscoverySource FromRaw(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new DiscoverySource("TRAFFIC");
        }

        return new DiscoverySource(raw);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    private static string Normalize(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        return Allowed.Contains(normalized) ? normalized : "OTHER";
    }
}
