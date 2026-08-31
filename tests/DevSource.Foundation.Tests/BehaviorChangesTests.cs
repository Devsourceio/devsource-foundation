using DevSource.Foundation.Abstractions;
using DevSource.Foundation.Application;
using DevSource.Foundation.Domain;
using DevSource.Foundation.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace DevSource.Foundation.Tests;

public sealed class BehaviorChangesTests
{
    [Fact]
    public async Task ApplicationService_DoesNotCommit_WhenOperationReturnsFailureResult()
    {
        var unitOfWork = new RecordingUnitOfWork();
        var service = new TestApplicationService(unitOfWork);

        var result = await service.ExecuteResultAsync(_ =>
            Task.FromResult(Result<string>.Failure(new Error("customer.invalid", "Invalid customer."))));

        Assert.True(result.IsFailure);
        Assert.Equal(0, unitOfWork.CommitCount);
    }

    [Fact]
    public void SpecificationPagination_RejectsInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpecificationPagination(-1, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpecificationPagination(0, 0));
        Assert.Throws<ArgumentException>(() => new SpecificationPagination(0, 10, " "));
        Assert.Throws<ArgumentException>(() => new SpecificationPagination(1, 10, "cursor"));
    }

    [Fact]
    public void SpecificationEvaluator_AppliesAscendingStringCursor()
    {
        var evaluator = new SpecificationEvaluator<Model>();
        var specification = new CursorSpecification("Bruno");
        var models = new[]
        {
            new Model("Alice"),
            new Model("Bruno"),
            new Model("Carla"),
        }.AsQueryable();

        var result = evaluator.Evaluate(models, specification).Select(model => model.Name).ToArray();

        Assert.Equal(["Carla"], result);
    }

    [Fact]
    public void ResultFailure_RejectsNullErrors()
    {
        Error?[] errors = [new Error("one", "One."), null];

        Assert.Throws<ArgumentException>(() => Result.Failure(errors!));
    }

    [Fact]
    public async Task DomainEventDispatcher_InvokesHandlersAndClearsEvents()
    {
        var aggregate = new TestAggregate();
        aggregate.RaiseTestEvent();
        var dispatcher = new DomainEventDispatcher();
        var handled = 0;
        dispatcher.Register<TestEvent>((_, _) =>
        {
            handled++;
            return Task.CompletedTask;
        });

        await dispatcher.DispatchAsync(aggregate, TestContext.Current.CancellationToken);

        Assert.Equal(1, handled);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public async Task DomainEventDispatcher_KeepsEventsWhenHandlerFails()
    {
        var aggregate = new TestAggregate();
        aggregate.RaiseTestEvent();
        var dispatcher = new DomainEventDispatcher();
        dispatcher.Register<TestEvent>((_, _) =>
            Task.FromException(new InvalidOperationException("handler failure")));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.DispatchAsync(aggregate, TestContext.Current.CancellationToken));

        Assert.Single(aggregate.DomainEvents);
    }

    [Fact]
    public async Task AddDomainEvents_AddHandler_ResolvesAndInvokesHandlerFromDependencyInjection()
    {
        var services = new ServiceCollection()
            .AddDomainEvents()
            .AddHandler<TestEvent, TestEventHandler>();
        await using var provider = services.BuildServiceProvider();
        var aggregate = new TestAggregate();
        aggregate.RaiseTestEvent();

        await provider.GetRequiredService<IDomainEventDispatcher>()
            .DispatchAsync(aggregate, TestContext.Current.CancellationToken);

        var handler = provider.GetRequiredService<TestEventHandler>();
        Assert.Equal(1, handler.HandledCount);
        Assert.Empty(aggregate.DomainEvents);
    }

    private sealed class TestApplicationService(IUnitOfWork unitOfWork) : ApplicationService(unitOfWork)
    {
        public Task<Result<T>> ExecuteResultAsync<T>(Func<CancellationToken, Task<Result<T>>> operation)
            => ExecuteAsync(operation);
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class CursorSpecification : Specification<Model>
    {
        public CursorSpecification(string cursor)
        {
            AllowField(model => model.Name);
            OrderBy(model => model.Name);
            Paginate(cursor, 10);
        }
    }

    private sealed record Model(string Name);

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate() : base(Guid.NewGuid()) { }

        public void RaiseTestEvent() => AddDomainEvent(new TestEvent());
    }

    private sealed class TestEvent : DomainEvent;

    private sealed class TestEventHandler : IDomainEventHandler<TestEvent>
    {
        public int HandledCount { get; private set; }

        public Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken = default)
        {
            HandledCount++;
            return Task.CompletedTask;
        }
    }
}
