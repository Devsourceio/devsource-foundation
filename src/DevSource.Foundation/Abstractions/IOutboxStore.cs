namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Persists and reads integration events for the Outbox Pattern.
/// </summary>
/// <remarks>
/// <see cref="AddAsync"/> must participate in the caller's current unit of work. The
/// implementation is responsible for concurrency control when multiple delivery workers
/// read pending messages.
/// </remarks>
public interface IOutboxStore
{
    /// <summary>
    /// Adds a new outbox message to the storage for future delivery.
    /// </summary>
    /// <param name="message">The outbox message to be persisted.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a batch of pending outbox messages for processing.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to retrieve in the batch.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing a read-only list of pending outbox messages.</returns>
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified outbox message as processed and updates the processed timestamp.
    /// </summary>
    /// <param name="message">The outbox message to be marked as processed.</param>
    /// <param name="processedOnUtc">The timestamp indicating when the message was processed, in UTC.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkProcessedAsync(OutboxMessage message, DateTimeOffset processedOnUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified outbox message as failed with an associated error message.
    /// </summary>
    /// <param name="message">The outbox message to be marked as failed.</param>
    /// <param name="error">The error description to associate with the failed message.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkFailedAsync(OutboxMessage message, string error, CancellationToken cancellationToken = default);
}
