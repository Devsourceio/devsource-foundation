namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Represents a serialized integration event waiting for delivery.
/// </summary>
/// <remarks>
/// The message is intentionally independent of a database provider. An infrastructure
/// implementation should persist the message together with the business changes in the
/// same transaction and update its delivery state in a later transaction.
/// </remarks>
public sealed class OutboxMessage
{
    /// <summary>
    /// Gets the unique identifier of the outbox message.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the name of the integration event type associated with the message.
    /// </summary>
    /// <remarks>
    /// This property typically represents the fully qualified class name of the event
    /// to allow proper deserialization and processing of the message by the consumer.
    /// </remarks>
    public string EventType { get; }

    /// <summary>
    /// Gets the serialized representation of the integration event associated with the outbox message.
    /// </summary>
    /// <remarks>
    /// This property contains the event's data in a structured format, typically JSON.
    /// It is used to deserialize and process the event during delivery.
    /// </remarks>
    public string Payload { get; }

    /// <summary>
    /// Gets the UTC timestamp indicating when the event occurred.
    /// </summary>
    /// <remarks>
    /// This property represents the exact time the event took place, recorded in UTC.
    /// It is essential for maintaining accurate event timelines, particularly in distributed systems.
    /// </remarks>
    public DateTimeOffset OccurredOnUtc { get; }

    /// <summary>
    /// Gets the current status of the outbox message.
    /// </summary>
    /// <remarks>
    /// The status indicates the state of the message in its lifecycle, such as
    /// whether it is pending delivery, has been successfully published, or has failed.
    /// </remarks>
    public OutboxMessageStatus Status { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp indicating when the outbox message was successfully processed.
    /// </summary>
    /// <remarks>
    /// This property is set when the message delivery is confirmed, marking the message as processed.
    /// If the message has not yet been processed, this value will be null.
    /// </remarks>
    public DateTimeOffset? ProcessedOnUtc { get; private set; }

    /// <summary>
    /// Gets the number of times the outbox message delivery has been attempted.
    /// </summary>
    /// <remarks>
    /// The value is incremented each time a delivery attempt fails.
    /// It can be used to implement retry policies or to track the delivery process.
    /// </remarks>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Gets the description of the last error that occurred during the delivery attempt.
    /// </summary>
    /// <remarks>
    /// This property holds the most recent error message encountered when a delivery
    /// attempt fails. It is updated whenever <see cref="OutboxMessage.MarkFailed(string)"/> is called
    /// and cleared when <see cref="OutboxMessage.MarkProcessed(DateTimeOffset)"/> is invoked.
    /// </remarks>
    public string? LastError { get; private set; }

    /// <summary>
    /// Represents a message intended to be stored and processed from an outbox mechanism.
    /// Used for ensuring reliable delivery of integration events in distributed systems.
    /// </summary>
    public OutboxMessage(Guid id, string eventType, string payload, DateTimeOffset occurredOnUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The message identifier cannot be empty.", nameof(id));
        }

        EventType = string.IsNullOrWhiteSpace(eventType)
            ? throw new ArgumentException("The event type is required.", nameof(eventType))
            : eventType;
        Payload = string.IsNullOrWhiteSpace(payload)
            ? throw new ArgumentException("The payload is required.", nameof(payload))
            : payload;
        Id = id;
        OccurredOnUtc = occurredOnUtc;
        Status = OutboxMessageStatus.Pending;
    }

    /// <summary>
    /// Marks the message as successfully processed by setting the processed timestamp and clearing any existing error message.
    /// This method is used to finalize the state of the message after successful handling.
    /// </summary>
    /// <param name="processedOnUtc">The date and time (in UTC) when the message was processed. This value updates the <see cref="ProcessedOnUtc"/> property.</param>
    public void MarkProcessed(DateTimeOffset processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Status = OutboxMessageStatus.Published;
        LastError = null;
    }

    /// <summary>
    /// Marks the message as failed to process and increments the retry count.
    /// Updates the <see cref="OutboxMessage.LastError"/> property with the provided error description.
    /// </summary>
    /// <param name="error">A non-empty string describing the error that caused the processing failure.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the provided <paramref name="error"/> is null, empty, or consists only of whitespace.
    /// </exception>
    public void MarkFailed(string error)
    {
        LastError = string.IsNullOrWhiteSpace(error)
            ? throw new ArgumentException("The delivery error is required.", nameof(error))
            : error;
        RetryCount++;
        Status = OutboxMessageStatus.Failed;
    }

    /// <summary>
    /// Attempts to retry processing a failed outbox message by resetting its status to pending.
    /// Clears the last recorded error, allowing the message to be retried.
    /// Throws an exception if the current status is not failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the message cannot be retried because its status is not 'Failed'.
    /// </exception>
    public void Retry()
    {
        if (Status != OutboxMessageStatus.Failed)
        {
            throw new InvalidOperationException("Only failed messages can be retried.");
        }

        Status = OutboxMessageStatus.Pending;
        LastError = null;
    }
}
