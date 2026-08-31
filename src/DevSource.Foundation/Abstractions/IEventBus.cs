namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Defines asynchronous publishing of integration events to an external delivery mechanism.
/// </summary>
/// <remarks>
/// Completion means that the configured implementation accepted the event for its delivery
/// workflow. It does not, by itself, guarantee that every external consumer processed the event.
/// Implementations must propagate delivery failures and honor cancellation; callers should await
/// this method and must not treat it as fire-and-forget. Delivery, retry, ordering, duplication,
/// and durability guarantees belong to the infrastructure implementation.
/// </remarks>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event.
    /// </summary>
    /// <typeparam name="TEvent">The event payload type.</typeparam>
    /// <param name="integrationEvent">The integration event payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="OperationCanceledException">Thrown when publication is canceled.</exception>
    /// <exception cref="Exception">Thrown when the event cannot be accepted by the delivery mechanism.</exception>
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class;
}
