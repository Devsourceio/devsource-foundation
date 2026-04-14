namespace DevSource.Stack.Application;

/// <summary>
/// Provides access to the current application user context.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the current user identifier.
    /// </summary>
    Guid UserId { get; }
}
