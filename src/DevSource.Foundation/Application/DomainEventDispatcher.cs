using DevSource.Foundation.Abstractions;
using DevSource.Foundation.Domain;

namespace DevSource.Foundation.Application;

/// <summary>
/// Provides simple in-process dispatch of domain events through explicitly registered handlers.
/// </summary>
/// <remarks>
/// Handler lookup is exact by event type and handlers run sequentially in registration order.
/// This dispatcher is intended for domain-side effects within the process. Integration events
/// should be created separately and published through <see cref="IEventBus"/>.
/// </remarks>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly Dictionary<Type, List<Func<DomainEvent, CancellationToken, Task>>> handlers = [];

    /// <summary>
    /// Registers a handler for a concrete domain event type.
    /// </summary>
    /// <typeparam name="TEvent">The domain event type.</typeparam>
    /// <param name="handler">Handler to invoke.</param>
    public void Register<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (!handlers.TryGetValue(typeof(TEvent), out var eventHandlers))
        {
            eventHandlers = [];
            handlers.Add(typeof(TEvent), eventHandlers);
        }

        eventHandlers.Add((domainEvent, cancellationToken) =>
            handler((TEvent)domainEvent, cancellationToken));
    }

    /// <inheritdoc />
    public async Task DispatchAsync<TId>(
        AggregateRoot<TId> aggregate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        foreach (var domainEvent in aggregate.DomainEvents.ToArray())
        {
            if (!handlers.TryGetValue(domainEvent.GetType(), out var eventHandlers))
            {
                continue;
            }

            foreach (var handler in eventHandlers)
            {
                await handler(domainEvent, cancellationToken).ConfigureAwait(false);
            }
        }

        aggregate.ClearDomainEvents();
    }
}
