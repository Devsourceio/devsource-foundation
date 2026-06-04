namespace DevSource.Foundation.Application;

/// <summary>
/// Provides access to the current tenant context.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the current tenant identifier.
    /// </summary>
    string TenantId { get; }
}
