namespace DevSource.Foundation.Primitives;

/// <summary>
/// This represents a domain/application error with stable code and a descriptive message.
/// </summary>
[Serializable]
public sealed class Error : IEquatable<Error>
{
/// <summary>
    /// Gets the stable code of the error.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the descriptive message of the error.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Represents the absence of an error.
    /// </summary>
    public static readonly Error None = new("none", "No error.");

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The stable code of the error.</param>
    /// <param name="message">The descriptive message of the error.</param>
    public Error(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Error code cannot be null or whitespace.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));
        }

        Code = code;
        Message = message;
    }    

    /// <summary>
    /// Compares two errors by their <see cref="Code"/> property.
    /// </summary>
    /// <param name="other">Another error to compare.</param>
    /// <returns><see langword="true"/> when the codes are equal.</returns>
    public bool Equals(Error? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Code, other.Code, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Error other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Code);
    }

    /// <summary>
    /// Compares two errors by their <see cref="Code"/> property.
    /// </summary>
    public static bool operator ==(Error? left, Error? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Compares two errors by their <see cref="Code"/> property.
    /// </summary>
    public static bool operator !=(Error? left, Error? right)
    {
        return !Equals(left, right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Code}: {Message}";
    }
}
