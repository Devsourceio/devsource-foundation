using DevSource.Foundation.Domain;

namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Dispatches domain events raised by an aggregate root.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches the pending events of an aggregate root.
    /// </summary>
    /// <typeparam name="TId">The aggregate identifier type.</typeparam>
    /// <param name="aggregate">Aggregate whose pending events will be dispatched.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes after all registered handlers finish.</returns>
    /// <remarks>
    /// Events are cleared only after all handlers complete successfully. If a handler fails,
    /// the events remain pending so the caller can decide whether to retry or discard them.
    /// </remarks>
    Task DispatchAsync<TId>(AggregateRoot<TId> aggregate, CancellationToken cancellationToken = default);
}
