using DevSource.Foundation.Abstractions;

namespace DevSource.Foundation.Application;

/// <summary>
/// Provides a lightweight base type for application orchestration.
/// </summary>
/// <remarks>
/// This type coordinates use-case execution and transaction boundaries without domain logic
/// and without direct dependency on dispatch frameworks.
/// </remarks>
public abstract class ApplicationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationService"/> class.
    /// </summary>
    /// <param name="unitOfWork">Transactional unit used to commit persistence changes.</param>
    protected ApplicationService(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets the transactional unit used by this application service.
    /// </summary>
    protected IUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Commits pending transactional changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return UnitOfWork.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Executes an asynchronous operation and commits on success.
    /// </summary>
    /// <param name="operation">Operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await operation(cancellationToken).ConfigureAwait(false);
        await UnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an asynchronous operation with result and commits on success.
    /// </summary>
    /// <typeparam name="TResult">Operation result type.</typeparam>
    /// <param name="operation">Operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The operation result.</returns>
    protected async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var result = await operation(cancellationToken).ConfigureAwait(false);
        await UnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }
}
