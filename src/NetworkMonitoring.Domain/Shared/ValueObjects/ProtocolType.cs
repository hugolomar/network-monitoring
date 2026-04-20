namespace NetworkMonitoring.Domain.ValueObjects;

public sealed class ProtocolType : ValueObject
{
    private static readonly HashSet<string> Allowed = ["TCP", "UDP", "ICMP", "OTHER"];

    public string Value { get; }

    public ProtocolType(string value)
    {
        Value = Normalize(value);
    }

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
