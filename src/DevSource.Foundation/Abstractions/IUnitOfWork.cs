namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Defines a transactional boundary for committing pending persistence changes.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits the current transactional unit.
    /// </summary>
    /// <remarks>
    /// A successful completion represents a successful commit. Implementations should report
    /// commit failures by throwing and should honor cancellation. Explicit rollback is not part
    /// of this contract because transaction ownership and rollback behavior belong to the
    /// persistence implementation unless an explicit transaction API is introduced.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);
}
