using DevSource.Foundation.Domain;

namespace DevSource.Foundation.Tests;

public sealed class DomainTests
{
    [Fact]
    public void DomainEvent_WithOccurredOnUtc_InitializesMetadata()
    {
        // Arrange
        var occurredOnUtc = new DateTimeOffset(2026, 4, 14, 12, 0, 0, TimeSpan.Zero);

        // Act
        var domainEvent = new SampleDomainEvent(occurredOnUtc);

        // Assert
        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
    }

    [Fact]
    public void DomainEvent_WithoutOccurredOnUtc_UsesCurrentUtcTime()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var domainEvent = new EmptyDomainEvent();

        // Assert
        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.InRange(domainEvent.OccurredOnUtc, before, DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Entity_Equals_ReturnsTrueForSameReference()
    {
        // Arrange
        var entity = new SampleEntity(Guid.NewGuid());
        var sameReference = entity;

        // Act
        var equals = entity.Equals(entity);
        var areEqual = entity == sameReference;

        // Assert
        Assert.True(equals);
        Assert.True(areEqual);
    }

    [Fact]
    public void Entity_Equals_ReturnsFalseForNullOrDifferentType()
    {
        // Arrange
        var entity = new SampleEntity(Guid.NewGuid());
        Entity<Guid>? other = null;
        var differentTypeEntity = new AnotherSampleEntity(entity.Id);

        // Act
        var equalsNull = entity.Equals(other);
        var equalsDifferentType = entity.Equals(differentTypeEntity);
        var notEqualOperator = entity != other;

        // Assert
        Assert.False(equalsNull);
        Assert.False(equalsDifferentType);
        Assert.True(notEqualOperator);
    }

    [Fact]
    public void Entity_Equals_ReturnsFalseForTransientEntities()
    {
        // Arrange
        var left = new SampleEntity();
        var right = new SampleEntity();

        // Act
        var equals = left.Equals(right);
        var hashCode = left.GetHashCode();

        // Assert
        Assert.False(equals);
        Assert.IsType<int>(hashCode);
    }

    [Fact]
    public void Entity_Equals_ReturnsTrueForSameIdentityAndType()
    {
        // Arrange
        var id = Guid.NewGuid();
        var left = new SampleEntity(id);
        var right = new SampleEntity(id);

        // Act
        var equalsTyped = left.Equals(right);
        var equalsObject = left.Equals((object)right);
        var leftHashCode = left.GetHashCode();
        var rightHashCode = right.GetHashCode();

        // Assert
        Assert.True(equalsTyped);
        Assert.True(equalsObject);
        Assert.Equal(leftHashCode, rightHashCode);
    }

    [Fact]
    public void Entity_EqualsObject_ReturnsFalseForDifferentObjectType()
    {
        // Arrange
        var entity = new SampleEntity(Guid.NewGuid());

        // Act
        var equals = entity.Equals("foundation");

        // Assert
        Assert.False(equals);
    }

    [Fact]
    public void ValueObject_Equals_ReturnsExpectedResults()
    {
        // Arrange
        var left = new SampleValueObject("foundation", 1);
        var same = new SampleValueObject("foundation", 1);
        var different = new SampleValueObject("foundation", 2);
        var differentType = new AnotherSampleValueObject("foundation", 1);

        // Act
        var equalsSelf = left!.Equals(left);
        var equalsTyped = left!.Equals(same);
        var equalsObject = left!.Equals((object)same);
        var equalsDifferentObject = left!.Equals(new object());
        var equalsDifferent = left!.Equals(different);
        var equalsDifferentType = left!.Equals(differentType);
        ValueObject? nullValueObject = null;
        var equalsNull = left!.Equals(nullValueObject);
        var equalOperator = left! == same!;
        var notEqualOperator = left! != different!;
        var leftHashCode = left!.GetHashCode();
        var sameHashCode = same!.GetHashCode();

        // Assert
        Assert.True(equalsSelf);
        Assert.True(equalsTyped);
        Assert.True(equalsObject);
        Assert.False(equalsDifferentObject);
        Assert.False(equalsDifferent);
        Assert.False(equalsDifferentType);
        Assert.False(equalsNull);
        Assert.True(equalOperator);
        Assert.True(notEqualOperator);
        Assert.Equal(leftHashCode, sameHashCode);
    }

    [Fact]
    public void AggregateRoot_AddsAndClearsDomainEvents()
    {
        // Arrange
        var aggregate = new SampleAggregateRoot(Guid.NewGuid());
        var domainEvent = new EmptyDomainEvent();

        // Act
        aggregate.Record(domainEvent);
        var eventsAfterRecord = aggregate.DomainEvents.ToArray();
        aggregate.ClearDomainEvents();

        // Assert
        Assert.Single(eventsAfterRecord);
        Assert.Contains(domainEvent, eventsAfterRecord);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void AggregateRoot_AddDomainEvent_ThrowsWhenEventIsNull()
    {
        // Arrange
        var aggregate = new SampleAggregateRoot(Guid.NewGuid());

        // Act
        var action = () => aggregate.Record(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void AggregateRoot_DefaultConstructors_KeepDefaultIdentifiers()
    {
        // Arrange
        var guidAggregate = new SampleAggregateRoot();
        var genericAggregate = new SampleStringAggregateRoot();
        var aggregateWithId = new SampleStringAggregateRoot("foundation");

        // Act
        var guidAggregateId = guidAggregate.Id;
        var genericAggregateId = genericAggregate.Id;
        var aggregateWithIdValue = aggregateWithId.Id;

        // Assert
        Assert.Equal(Guid.Empty, guidAggregateId);
        Assert.Null(genericAggregateId);
        Assert.Equal("foundation", aggregateWithIdValue);
        Assert.Empty(guidAggregate.DomainEvents);
        Assert.Empty(genericAggregate.DomainEvents);
        Assert.Empty(aggregateWithId.DomainEvents);
    }

    private sealed class EmptyDomainEvent : DomainEvent
    {
    }

    private sealed class SampleDomainEvent(DateTimeOffset occurredOnUtc) : DomainEvent(occurredOnUtc)
    {
    }

    private sealed class SampleEntity : Entity<Guid>
    {
        public SampleEntity() { }

        public SampleEntity(Guid id) : base(id) { }
    }

    private sealed class AnotherSampleEntity(Guid id) : Entity<Guid>(id)
    {
    }

    private sealed class SampleValueObject(string name, int version) : ValueObject
    {
        public string Name { get; } = name;

        public int Version { get; } = version;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Name;
            yield return Version;
        }
    }

    private sealed class AnotherSampleValueObject(string name, int version) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return name;
            yield return version;
        }
    }

    private sealed class SampleAggregateRoot : AggregateRoot
    {
        public SampleAggregateRoot() { }

        public SampleAggregateRoot(Guid id) : base(id) { }

        public void Record(DomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }
    }

    private sealed class SampleStringAggregateRoot : AggregateRoot<string>
    {
        public SampleStringAggregateRoot() { }

        public SampleStringAggregateRoot(string id) : base(id) { }
    }
}
