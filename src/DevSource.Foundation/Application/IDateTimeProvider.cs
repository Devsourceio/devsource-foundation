namespace DevSource.Stack.Application;

/// <summary>
/// Provides UTC date and time values for application use cases.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
