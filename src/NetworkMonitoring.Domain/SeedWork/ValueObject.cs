namespace NetworkMonitoring.Domain.SeedWork;

/// <summary>
/// Base class for Value Objects in the Domain-Driven Design model.
/// Value objects are compared by their properties/components, not by identity.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Compares two value objects for equality.
    /// </summary>
    /// <param name="left">The left value object.</param>
    /// <param name="right">The right value object.</param>
    /// <returns>True if the objects are equal; otherwise, false.</returns>
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
        {
            return false;
        }
        return ReferenceEquals(left, null) || left.Equals(right);
    }

    /// <summary>
    /// Compares two value objects for inequality.
    /// </summary>
    /// <param name="left">The left value object.</param>
    /// <param name="right">The right value object.</param>
    /// <returns>True if the objects are not equal; otherwise, false.</returns>
    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !(EqualOperator(left, right));
    }

    /// <summary>
    /// Gets the components of the value object that dictate its equality.
    /// </summary>
    /// <returns>A sequence of components.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current value object.</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Gets a shallow copy of the value object.
    /// </summary>
    /// <returns>A copy of the value object.</returns>
    public ValueObject GetCopy()
    {
        return this.MemberwiseClone() as ValueObject;
    }
}