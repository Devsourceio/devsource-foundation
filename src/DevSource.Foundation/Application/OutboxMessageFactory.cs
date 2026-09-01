using System.Text.Json;
using DevSource.Foundation.Abstractions;

namespace DevSource.Foundation.Application;

/// <summary>
/// Creates durable Outbox envelopes from integration-event instances.
/// </summary>
public static class OutboxMessageFactory
{
    /// <summary>
    /// Serializes an integration event using its full type name as discriminator.
    /// </summary>
    /// <typeparam name="TEvent">The integration-event type.</typeparam>
    /// <param name="integrationEvent">Event to serialize.</param>
    /// <param name="occurredOnUtc">Event occurrence time.</param>
    /// <param name="jsonOptions">Optional JSON options.</param>
    /// <returns>A new pending Outbox message.</returns>
    public static OutboxMessage Create<TEvent>(
        TEvent integrationEvent,
        DateTimeOffset occurredOnUtc,
        JsonSerializerOptions? jsonOptions = null)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var eventType = integrationEvent.GetType().FullName
            ?? throw new ArgumentException("The event type must have a name.", nameof(integrationEvent));
        var payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), jsonOptions);

        return new OutboxMessage(Guid.CreateVersion7(DateTimeOffset.UtcNow), eventType, payload, occurredOnUtc);
    }
}
