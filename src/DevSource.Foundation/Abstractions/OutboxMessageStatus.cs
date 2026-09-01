namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Defines the statuses that an outbox message can have during its lifecycle.
/// </summary>
public enum OutboxMessageStatus
{
    /// <summary>
    /// Indicates that the message is awaiting processing or initial delivery.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Indicates that the message has been successfully delivered or processed.
    /// </summary>
    Published = 1,

    /// <summary>
    /// Indicates that the message delivery has failed and cannot be processed successfully.
    /// </summary>
    Failed = 2
}
