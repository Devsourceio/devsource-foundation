namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Defines a transactional boundary for committing pending persistence changes.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits the current transactional unit.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);
}
