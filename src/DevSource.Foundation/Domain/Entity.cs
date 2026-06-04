namespace DevSource.Foundation.Domain;

/// <summary>
/// Represents a base entity with a generic identifier type.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
{
    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    public TId Id { get; protected init; } = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// </summary>
    protected Entity() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class with an identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    protected Entity(TId id)
    {
        Id = id;
    }    

    /// <summary>
    /// Determines whether this entity and another entity are equal by identity.
    /// </summary>
    /// <param name="other">The other entity to compare.</param>
    /// <returns><see langword="true"/> when both entities have the same non-default identifier and type.</returns>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        if (IsTransient() || other.IsTransient()) return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return IsTransient()
            ? base.GetHashCode()
            : HashCode.Combine(GetType(), Id);
    }

    /// <summary>
    /// Compares two entities by identity.
    /// </summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Compares two entities by identity inequality.
    /// </summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Determines whether the entity has a default identifier.
    /// </summary>
    /// <returns><see langword="true"/> when the identifier is default.</returns>
    protected bool IsTransient()
    {
        return EqualityComparer<TId>.Default.Equals(Id, default!);
    }
}

/// <summary>
/// Represents a base entity class with a Guid as the default identifier type.
/// </summary>
public abstract class Entity : Entity<Guid>
{
    /// <summary>
    /// Represents a base entity class with a Guid as the default identifier type.
    /// </summary>
    protected Entity()
    {
    }

    /// <summary>
    /// Represents a base entity class with a Guid as the default identifier type.
    /// </summary>
    protected Entity(Guid id) : base(id)
    {
    }
}
