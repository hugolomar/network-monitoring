namespace NetworkMonitoring.Domain.ValueObjects;

/// <summary>
/// Represents a validated network port (1-65535).
/// </summary>
public sealed class Port : ValueObject
{
    /// <summary>
    /// Gets the port number.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Port"/> class.
    /// </summary>
    /// <param name="value">The port number.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the port is outside the valid range.</exception>
    public Port(int value)
    {
        if (value is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Port must be between 1 and 65535.");
        }

        Value = value;
    }

    /// <summary>
    /// Tries to create a <see cref="Port"/> instance from a nullable integer.
    /// </summary>
    /// <param name="value">The raw port number.</param>
    /// <param name="port">The resulting <see cref="Port"/> or null if invalid.</param>
    /// <returns>True if creation was successful; otherwise, false.</returns>
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

    /// <summary>
    /// Gets the components used for equality comparison.
    /// </summary>
    /// <returns>An enumeration of equality components.</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns the string representation of the port number.
    /// </summary>
    /// <returns>The port value as a string.</returns>
    public override string ToString() => Value.ToString();
}
