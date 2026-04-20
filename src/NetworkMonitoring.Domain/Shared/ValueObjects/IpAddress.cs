using System.Net;

namespace NetworkMonitoring.Domain.ValueObjects;

public sealed class IpAddress : ValueObject
{
    public string Value { get; }

    public IpAddress(string value)
    {
        if (!TryNormalize(value, out var normalized))
        {
            throw new ArgumentException("Invalid IP address.", nameof(value));
        }

        Value = normalized;
    }

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

        if (!IPAddress.TryParse(value.Trim(), out var ip))
        {
            return false;
        }

        normalized = ip.ToString();
        return true;
    }
}
