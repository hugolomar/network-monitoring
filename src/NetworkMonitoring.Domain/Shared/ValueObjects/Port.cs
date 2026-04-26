namespace NetworkMonitoring.Domain.ValueObjects;

public sealed class Port : ValueObject
{
    public int Value { get; }

    public Port(int value)
    {
        if (value is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Port must be between 1 and 65535.");
        }

        Value = value;
    }

    public static bool TryCreate(int? value, out Port? port)
    {
        port = null;
        if (!value.HasValue || value.Value is < 1 or > 65535)
        {
            return false;
        }

        port = new Port(value.Value);
        return true;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
