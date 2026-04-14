namespace DevSource.Foundation.Domain;

/// <summary>
/// Represents a value object compared by structural equality.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the atomic values that define equality for this value object.
    /// </summary>
    /// <returns>The sequence of equality components.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Determines whether this instance and another value object are equal.
    /// </summary>
    /// <param name="other">Another value object to compare.</param>
    /// <returns><see langword="true"/> when both instances have the same type and components.</returns>
    public bool Equals(ValueObject? other)
    {
        if (other is null) return false;

        if (ReferenceEquals(this, other))return true;

        return GetType() == other.GetType() &&
               GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ValueObject other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Compares two value objects for equality.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Compares two value objects for inequality.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}
