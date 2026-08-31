using DevSource.Foundation.Domain;

namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Handles a concrete domain event.
/// </summary>
/// <typeparam name="TEvent">The domain event type.</typeparam>
public interface IDomainEventHandler<in TEvent>
    where TEvent : DomainEvent
{
    /// <summary>
    /// Handles the domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when handling finishes.</returns>
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
