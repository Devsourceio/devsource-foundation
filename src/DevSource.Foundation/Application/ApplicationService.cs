using DevSource.Foundation.Abstractions;
using DevSource.Foundation.Domain;
using DevSource.Foundation.Primitives;

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
    /// Dispatches pending domain events after the application operation has completed.
    /// </summary>
    /// <typeparam name="TId">The aggregate identifier type.</typeparam>
    /// <param name="aggregate">Aggregate containing pending domain events.</param>
    /// <param name="dispatcher">Domain event dispatcher.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected static Task DispatchDomainEventsAsync<TId>(
        AggregateRoot<TId> aggregate,
        IDomainEventDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ArgumentNullException.ThrowIfNull(dispatcher);
        return dispatcher.DispatchAsync(aggregate, cancellationToken);
    }

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
    /// <remarks>
    /// When the result is a <see cref="Result"/> (including <see cref="Result{TResult}"/>),
    /// failed results are returned without committing. Other result types commit after normal
    /// completion; thrown exceptions also prevent this method from reaching the commit.
    /// </remarks>
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

        // Result failures are expected business outcomes, not successful transactional operations.
        // They must not persist pending changes merely because the delegate completed without throwing.
        if (result is not Result { IsFailure: true })
        {
            await UnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        return result;
    }
}
