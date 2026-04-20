using System.Text.RegularExpressions;

namespace NetworkMonitoring.Domain.ValueObjects;

public sealed class MacAddress : ValueObject
{
    private static readonly Regex HexRegex = new("^[0-9A-F]{12}$", RegexOptions.Compiled);

    public string Value { get; }

    public MacAddress(string value)
    {
        if (!TryNormalize(value, out var normalized))
        {
            throw new ArgumentException("Invalid MAC address.", nameof(value));
        }

        Value = normalized;
    }

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

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

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
