namespace DevSource.Foundation.Primitives;

/// <summary>
/// Provides factory members to create optional values without using null.
/// </summary>
public static class Maybe
{
    /// <summary>
    /// Gets a marker that represents no value.
    /// </summary>
    public static MaybeNone None => default;

    /// <summary>
    /// Creates an optional value from a non-null instance.
    /// </summary>
    /// <typeparam name="T">The wrapped value type.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A <see cref="Maybe{T}"/> with value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static Maybe<T> From<T>(T value)
    {
        return Maybe<T>.From(value);
    }
}

/// <summary>
/// Represents an explicit no-value marker for <see cref="Maybe{T}"/>.
/// </summary>
public readonly struct MaybeNone;

/// <summary>
/// Represents an optional value that may or may not be present.
/// </summary>
/// <typeparam name="T">The wrapped value type.</typeparam>
public readonly struct Maybe<T> : IEquatable<Maybe<T>>
{
    private readonly T? _value;

    private Maybe(T value)
    {
        _value = value;
        HasValue = true;
    }

    /// <summary>
    /// Gets an empty optional value.
    /// </summary>
    public static Maybe<T> None => default;

    /// <summary>
    /// Gets a value indicating whether an instance contains a value.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets the wrapped value when present.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no value is present.</exception>
    public T Value => HasValue
        ? _value!
        : throw new InvalidOperationException("Cannot access value when Maybe has no value.");

    /// <summary>
    /// Creates an optional value from a non-null instance.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A <see cref="Maybe{T}"/> with value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static Maybe<T> From(T value)
    {
        return value is null 
            ? throw new ArgumentNullException(nameof(value)) 
            : new Maybe<T>(value);
    }

    /// <summary>
    /// Converts a no-value marker to an empty optional value.
    /// </summary>
    /// <param name="none">No-value marker.</param>
    public static implicit operator Maybe<T>(MaybeNone none) { return None; }

    /// <inheritdoc />
    public bool Equals(Maybe<T> other)
    {
        if (HasValue != other.HasValue) { return false; }
        return !HasValue || EqualityComparer<T>.Default.Equals(_value!, other._value!);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Maybe<T> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return !HasValue ? 0 : EqualityComparer<T>.Default.GetHashCode(_value!);
    }

    /// <summary>
    /// Compares two optional values for equality.
    /// </summary>
    public static bool operator ==(Maybe<T> left, Maybe<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Compares two optional values for inequality.
    /// </summary>
    public static bool operator !=(Maybe<T> left, Maybe<T> right)
    {
        return !(left == right);
    }
}
