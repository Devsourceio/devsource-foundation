namespace DevSource.Foundation.Primitives;

/// <summary>
/// Provides fail-fast guard clauses for programmer errors at system boundaries.
/// </summary>
/// <remarks>
/// Use guard clauses for invalid method usage and invariant violations that should stop execution immediately.
/// Use <see cref="Result"/> for expected business validation failures that should be returned to the caller.
/// </remarks>
public static class Guard
{
    /// <summary>
    /// Ensures the provided reference is not null.
    /// </summary>
    /// <typeparam name="T">Reference type being validated.</typeparam>
    /// <param name="value">Value to validate.</param>
    /// <param name="paramName">Parameter name.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <returns>The same non-null value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static T NotNull<T>(T? value, string paramName, string? message = null) where T : class
    {
        return value ?? throw new ArgumentNullException(paramName, message ?? $"Parameter '{paramName}' cannot be null.");
    }

    /// <summary>
    /// Ensures the provided string is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    /// <param name="paramName">Parameter name.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <returns>The same validated string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null, empty, or whitespace.</exception>
    public static string NotEmpty(string? value, string paramName, string? message = null)
    {
        return string.IsNullOrWhiteSpace(value) 
            ? throw new ArgumentException(message ?? $"Parameter '{paramName}' cannot be null, empty, or whitespace.", paramName) 
            : value;
    }

    /// <summary>
    /// Ensures the provided value is not the default value for its type.
    /// </summary>
    /// <typeparam name="T">Value type being validated.</typeparam>
    /// <param name="value">Value to validate.</param>
    /// <param name="paramName">Parameter name.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <returns>The same validated value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is the default value.</exception>
    public static T NotDefault<T>(T value, string paramName, string? message = null) where T : struct
    {
        return EqualityComparer<T>.Default.Equals(value, default) 
            ? throw new ArgumentException(message ?? $"Parameter '{paramName}' cannot be the default value.", paramName) 
            : value;
    }
}
