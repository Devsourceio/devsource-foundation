namespace DevSource.Foundation.Domain;

/// <summary>
/// Represents an immutable domain event that has already happened.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DomainEvent"/> class with a custom occurrence time.
/// </remarks>
/// <param name="occurredOnUtc">The UTC time when the event occurred.</param>
[Serializable]
public abstract class DomainEvent(DateTimeOffset occurredOnUtc)
{
    /// <summary>
    /// Gets the unique event identifier.
    /// </summary>
    public Guid EventId { get; } = Guid.CreateVersion7(DateTimeOffset.UtcNow);

    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset OccurredOnUtc { get; } = occurredOnUtc;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent"/> class.
    /// </summary>
    protected DomainEvent() : this(DateTimeOffset.UtcNow) {}
}
