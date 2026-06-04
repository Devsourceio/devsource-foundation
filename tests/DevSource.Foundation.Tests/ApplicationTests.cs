using System.Linq.Expressions;
using DevSource.Foundation.Abstractions;
using DevSource.Foundation.Application;

namespace DevSource.Foundation.Tests;

public sealed class ApplicationTests
{
    [Fact]
    public void ApplicationService_Constructor_ThrowsWhenUnitOfWorkIsNull()
    {
        // Act
        var action = () =>
        {
            _ = new TestApplicationService(null!);
        };

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public async Task ApplicationService_CommitAsync_DelegatesToUnitOfWork()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var service = new TestApplicationService(unitOfWork);
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        await service.CommitPublicAsync(cancellationToken);

        // Assert
        Assert.Equal(1, unitOfWork.CommitCount);
        Assert.Equal(cancellationToken, unitOfWork.LastCancellationToken);
    }

    [Fact]
    public async Task ApplicationService_ExecuteAsync_CommitsAfterSuccessfulOperation()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var service = new TestApplicationService(unitOfWork);
        var operationExecuted = false;
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        await service.ExecutePublicAsync(token =>
        {
            operationExecuted = token == cancellationToken;
            return Task.CompletedTask;
        }, cancellationToken);

        // Assert
        Assert.True(operationExecuted);
        Assert.Equal(1, unitOfWork.CommitCount);
        Assert.Equal(cancellationToken, unitOfWork.LastCancellationToken);
    }

    [Fact]
    public async Task ApplicationService_ExecuteAsyncOfT_ReturnsResultAndCommits()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var service = new TestApplicationService(unitOfWork);

        // Act
        var result = await service.ExecutePublicAsync(_ => Task.FromResult("foundation"), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("foundation", result);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    [Fact]
    public async Task ApplicationService_ExecuteAsync_DoesNotCommitWhenOperationFails()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var service = new TestApplicationService(unitOfWork);

        // Act
        var action = () => service.ExecutePublicAsync(_ => Task.FromException(new InvalidOperationException("failure")));

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Equal(0, unitOfWork.CommitCount);
    }

    [Fact]
    public async Task ApplicationService_ExecuteAsync_ThrowsWhenOperationIsNull()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var service = new TestApplicationService(unitOfWork);

        // Act
        var action = () => service.ExecutePublicAsync(null!);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(action);
    }

    [Fact]
    public async Task ApplicationService_ExecuteAsyncOfT_ThrowsWhenOperationIsNull()
    {
        // Arrange
        var unitOfWork = new FakeUnitOfWork();
        var service = new TestApplicationService(unitOfWork);

        // Act
        var action = () => service.ExecutePublicAsync<string>(null!);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(action);
    }

    [Fact]
    public void CompositeSpecification_StoresCompositionAndChildren()
    {
        // Arrange
        var left = new NameEqualsSpecification("Alice");
        var right = new AgeGreaterThanSpecification(18);

        // Act
        var specification = new CompositeSpecification<TestModel>(SpecificationComposition.And, left, right);

        // Assert
        Assert.Equal(SpecificationComposition.And, specification.Composition);
        Assert.Equal(2, specification.Children.Count);
        Assert.Empty(specification.Filters);
        Assert.Empty(specification.Orders);
        Assert.Empty(specification.AllowedFields);
        Assert.Null(specification.Pagination);
        Assert.Null(specification.Projection);
    }

    [Fact]
    public void CompositeSpecification_ThrowsForInvalidArguments()
    {
        // Arrange
        var child = new NameEqualsSpecification("Alice");

        // Act
        var noneAction = () =>
        {
            _ = new CompositeSpecification<TestModel>(SpecificationComposition.None, child);
        };
        var emptyChildrenAction = () =>
        {
            _ = new CompositeSpecification<TestModel>(SpecificationComposition.And, Array.Empty<ISpecification<TestModel>>());
        };
        var invalidNotAction = () =>
        {
            _ = new CompositeSpecification<TestModel>(SpecificationComposition.Not, child, child);
        };

        // Assert
        Assert.Throws<ArgumentException>(noneAction);
        Assert.Throws<ArgumentException>(emptyChildrenAction);
        Assert.Throws<ArgumentException>(invalidNotAction);
    }

    [Fact]
    public void SpecificationFieldResolver_Resolve_ReturnsCachedField()
    {
        // Arrange
        Expression<Func<TestModel, object>> expression = model => model.Address.City;

        // Act
        var first = SpecificationFieldResolver.Resolve(expression);
        var second = SpecificationFieldResolver.Resolve(expression);

        // Assert
        Assert.Same(first, second);
        Assert.Equal("City", first.Name);
        Assert.Equal("Address.City", first.Path);
        Assert.Equal(typeof(object), first.Type);
    }

    [Fact]
    public void SpecificationFieldResolver_ResolvePath_HandlesConvertExpressions()
    {
        // Arrange
        Expression<Func<TestModel, object>> expression = model => model.Age;

        // Act
        var path = SpecificationFieldResolver.ResolvePath(expression.Body);

        // Assert
        Assert.Equal("Age", path);
    }

    [Fact]
    public void SpecificationFieldResolver_Resolve_ThrowsForNullExpression()
    {
        // Act
        var action = () =>
        {
            _ = SpecificationFieldResolver.Resolve<TestModel, string>(null!);
        };

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void SpecificationFieldResolver_ResolvePath_ThrowsForUnsupportedExpression()
    {
        // Arrange
        Expression<Func<TestModel, bool>> expression = model => model.Age > 18;

        // Act
        var action = () =>
        {
            _ = SpecificationFieldResolver.ResolvePath(expression.Body);
        };

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void SpecificationProjection_StoresSelector_AndThrowsForNull()
    {
        // Arrange
        Expression<Func<TestModel, string>> selector = model => model.Name;

        // Act
        var projection = new SpecificationProjection<TestModel, string>(selector);
        var action = () =>
        {
            _ = new SpecificationProjection<TestModel, string>(null!);
        };

        // Assert
        Assert.Same(selector, projection.Selector);
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void Specification_BuildsFiltersOrdersPaginationAndProjection()
    {
        // Act
        var specification = new ConfiguredSpecification();

        // Assert
        Assert.Equal(2, specification.Filters.Count);
        Assert.Equal(2, specification.Orders.Count);
        Assert.Equal(2, specification.AllowedFields.Count);
        Assert.NotNull(specification.Pagination);
        Assert.Equal(0, specification.Pagination!.Skip);
        Assert.Equal(5, specification.Pagination.Take);
        Assert.Equal("cursor-1", specification.Pagination.Cursor);
        Assert.NotNull(specification.Projection);
    }

    [Fact]
    public void Specification_ThrowsWhenNoAllowedFieldWasDeclared()
    {
        // Act
        var action = () =>
        {
            _ = new MissingAllowFieldSpecification();
        };

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void Specification_ThrowsWhenFieldIsNotAllowed()
    {
        // Act
        var action = () =>
        {
            _ = new DisallowedFieldSpecification();
        };

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void Specification_Paginate_ThrowsForInvalidArguments()
    {
        // Act
        var negativeSkipAction = () =>
        {
            _ = new InvalidOffsetPaginationSpecification(-1, 1);
        };
        var zeroTakeAction = () =>
        {
            _ = new InvalidOffsetPaginationSpecification(0, 0);
        };
        var invalidCursorAction = () =>
        {
            _ = new InvalidCursorPaginationSpecification(0);
        };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(negativeSkipAction);
        Assert.Throws<ArgumentOutOfRangeException>(zeroTakeAction);
        Assert.Throws<ArgumentOutOfRangeException>(invalidCursorAction);
    }

    [Fact]
    public void Specification_CompositionMethods_CreateCompositeSpecifications()
    {
        // Arrange
        var left = new NameEqualsSpecification("Alice");
        var right = new AgeGreaterThanSpecification(18);

        // Act
        var andSpecification = left.And(right);
        var orSpecification = left.Or(right);
        var notSpecification = left.Not();

        // Assert
        Assert.Equal(SpecificationComposition.And, andSpecification.Composition);
        Assert.Equal(SpecificationComposition.Or, orSpecification.Composition);
        Assert.Equal(SpecificationComposition.Not, notSpecification.Composition);
    }

    [Fact]
    public void Specification_CompositionMethods_ThrowForNullSpecification()
    {
        // Arrange
        var specification = new NameEqualsSpecification("Alice");

        // Act
        var andAction = () =>
        {
            _ = specification.And(null!);
        };
        var orAction = () =>
        {
            _ = specification.Or(null!);
        };

        // Assert
        Assert.Throws<ArgumentNullException>(andAction);
        Assert.Throws<ArgumentNullException>(orAction);
    }

    [Fact]
    public void SpecificationEvaluator_Evaluate_AppliesFiltersOrderingAndPagination()
    {
        // Arrange
        var evaluator = new SpecificationEvaluator<TestModel>();
        var specification = new OrderedPagedAdultsSpecification();
        var query = CreateModels().AsQueryable();

        // Act
        var result = evaluator.Evaluate(query, specification).ToArray();

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal("Carla", result[0].Name);
        Assert.Equal("Alice", result[1].Name);
    }

    [Fact]
    public void SpecificationEvaluator_Evaluate_WithProjection_ReturnsProjectedValues()
    {
        // Arrange
        var evaluator = new SpecificationEvaluator<TestModel>();
        var specification = new NameProjectionSpecification();
        var query = CreateModels().AsQueryable();

        // Act
        var result = evaluator.Evaluate<string>(query, specification).ToArray();

        // Assert
        Assert.Equal(["Alice", "Carla", "Daniel"], result);
    }

    [Fact]
    public void SpecificationEvaluator_Evaluate_HandlesComposedSpecifications()
    {
        // Arrange
        var evaluator = new SpecificationEvaluator<TestModel>();
        var query = CreateModels().AsQueryable();
        var andSpecification = new AgeGreaterThanSpecification(20).And(new NameContainsSpecification("a"));
        var orSpecification = new NameEqualsSpecification("Alice").Or(new NameEqualsSpecification("Bruno"));
        var notSpecification = new NameEqualsSpecification("Bruno").Not();

        // Act
        var andResult = evaluator.Evaluate(query, andSpecification).Select(model => model.Name).ToArray();
        var orResult = evaluator.Evaluate(query, orSpecification).Select(model => model.Name).ToArray();
        var notResult = evaluator.Evaluate(query, notSpecification).Select(model => model.Name).ToArray();

        // Assert
        Assert.Equal(["Carla", "Daniel"], andResult);
        Assert.Equal(["Alice", "Bruno"], orResult);
        Assert.Equal(["Alice", "Carla", "Daniel"], notResult);
    }

    [Fact]
    public void SpecificationEvaluator_Evaluate_SupportsAllOperators()
    {
        // Arrange
        var evaluator = new SpecificationEvaluator<TestModel>();
        var query = CreateModels().AsQueryable();

        // Act
        var equalResult = evaluator.Evaluate(query, new CustomSpecification<string>(SpecificationFilterOperator.Equal, "Alice", model => model.Name)).Single().Name;
        var notEqualResult = evaluator.Evaluate(query, new CustomSpecification<string>(SpecificationFilterOperator.NotEqual, "Alice", model => model.Name)).Select(model => model.Name).ToArray();
        var greaterThanResult = evaluator.Evaluate(query, new CustomSpecification<int>(SpecificationFilterOperator.GreaterThan, 20, model => model.Age)).Select(model => model.Name).ToArray();
        var greaterThanOrEqualResult = evaluator.Evaluate(query, new CustomSpecification<int>(SpecificationFilterOperator.GreaterThanOrEqual, 30, model => model.Age)).Select(model => model.Name).ToArray();
        var lessThanResult = evaluator.Evaluate(query, new CustomSpecification<int>(SpecificationFilterOperator.LessThan, 30, model => model.Age)).Select(model => model.Name).ToArray();
        var lessThanOrEqualResult = evaluator.Evaluate(query, new CustomSpecification<int>(SpecificationFilterOperator.LessThanOrEqual, 20, model => model.Age)).Select(model => model.Name).ToArray();
        var containsResult = evaluator.Evaluate(query, new CustomSpecification<string>(SpecificationFilterOperator.Contains, "a", model => model.Name)).Select(model => model.Name).ToArray();
        var startsWithResult = evaluator.Evaluate(query, new CustomSpecification<string>(SpecificationFilterOperator.StartsWith, "A", model => model.Name)).Single().Name;
        var endsWithResult = evaluator.Evaluate(query, new CustomSpecification<string>(SpecificationFilterOperator.EndsWith, "l", model => model.Name)).Single().Name;

        // Assert
        Assert.Equal("Alice", equalResult);
        Assert.DoesNotContain("Alice", notEqualResult);
        Assert.Equal(["Alice", "Carla", "Daniel"], greaterThanResult);
        Assert.Equal(["Alice", "Carla", "Daniel"], greaterThanOrEqualResult);
        Assert.Equal(["Bruno"], lessThanResult);
        Assert.Equal(["Bruno"], lessThanOrEqualResult);
        Assert.Equal(["Carla", "Daniel"], containsResult);
        Assert.Equal("Alice", startsWithResult);
        Assert.Equal("Daniel", endsWithResult);
    }

    [Fact]
    public void SpecificationEvaluator_Evaluate_ThrowsForInvalidArgumentsAndProjection()
    {
        // Arrange
        var evaluator = new SpecificationEvaluator<TestModel>();
        var query = CreateModels().AsQueryable();
        var specification = new NameEqualsSpecification("Alice");

        // Act
        var nullQueryAction = () =>
        {
            _ = evaluator.Evaluate(null!, specification);
        };
        var nullSpecificationAction = () =>
        {
            _ = evaluator.Evaluate(query, null!);
        };
        var missingProjectionAction = () =>
        {
            _ = evaluator.Evaluate<string>(query, specification);
        };
        var incompatibleProjectionAction = () =>
        {
            _ = evaluator.Evaluate<int>(query, new NameProjectionSpecification());
        };

        // Assert
        Assert.Throws<ArgumentNullException>(nullQueryAction);
        Assert.Throws<ArgumentNullException>(nullSpecificationAction);
        Assert.Throws<InvalidOperationException>(missingProjectionAction);
        Assert.Throws<InvalidOperationException>(incompatibleProjectionAction);
    }

    [Fact]
    public void SpecificationEvaluator_Evaluate_ThrowsForInvalidOperatorAndFieldPath()
    {
        // Arrange
        var evaluator = new SpecificationEvaluator<TestModel>();
        var query = CreateModels().AsQueryable();

        // Act
        var invalidOperatorAction = () =>
        {
            _ = evaluator.Evaluate(query, new InvalidOperatorSpecification()).ToArray();
        };
        var invalidFieldPathAction = () =>
        {
            _ = evaluator.Evaluate(query, new EmptyFieldPathSpecification()).ToArray();
        };
        var invalidStringOperatorAction = () =>
        {
            _ = evaluator.Evaluate(query, new NonStringContainsSpecification()).ToArray();
        };

        // Assert
        Assert.Throws<InvalidOperationException>(invalidOperatorAction);
        Assert.Throws<InvalidOperationException>(invalidFieldPathAction);
        Assert.Throws<InvalidOperationException>(invalidStringOperatorAction);
    }

    [Fact]
    public void ApplicationContracts_ExposeExpectedMembers()
    {
        // Arrange
        var currentUserType = typeof(ICurrentUser);
        var dateTimeProviderType = typeof(IDateTimeProvider);
        var tenantProviderType = typeof(ITenantProvider);

        // Act
        var currentUserProperty = currentUserType.GetProperty(nameof(ICurrentUser.UserId));
        var utcNowProperty = dateTimeProviderType.GetProperty(nameof(IDateTimeProvider.UtcNow));
        var tenantProperty = tenantProviderType.GetProperty(nameof(ITenantProvider.TenantId));

        // Assert
        Assert.NotNull(currentUserProperty);
        Assert.NotNull(utcNowProperty);
        Assert.NotNull(tenantProperty);
    }

    private static List<TestModel> CreateModels()
    {
        return
        [
            new TestModel { Name = "Alice", Age = 30, Address = new Address { City = "Sao Paulo" } },
            new TestModel { Name = "Bruno", Age = 20, Address = new Address { City = "Rio" } },
            new TestModel { Name = "Carla", Age = 35, Address = new Address { City = "Curitiba" } },
            new TestModel { Name = "Daniel", Age = 40, Address = new Address { City = "Recife" } },
        ];
    }

    private sealed class TestApplicationService(IUnitOfWork unitOfWork) : ApplicationService(unitOfWork)
    {
        public Task CommitPublicAsync(CancellationToken cancellationToken = default) => CommitAsync(cancellationToken);

        public Task ExecutePublicAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
            => ExecuteAsync(operation, cancellationToken);

        public Task<TResult> ExecutePublicAsync<TResult>(Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default)
            => ExecuteAsync(operation, cancellationToken);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int CommitCount { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            LastCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class ConfiguredSpecification : Specification<TestModel>
    {
        public ConfiguredSpecification()
        {
            AllowField(model => model.Name);
            AllowField(model => model.Age);
            Where(model => model.Name, SpecificationFilterOperator.Contains, "a");
            Where(model => model.Age, SpecificationFilterOperator.GreaterThan, 18);
            OrderBy(model => model.Name);
            OrderByDescending(model => model.Age);
            Paginate(1, 10);
            Paginate("cursor-1", 5);
            Select(model => model.Name);
        }
    }

    private sealed class MissingAllowFieldSpecification : Specification<TestModel>
    {
        public MissingAllowFieldSpecification()
        {
            Where(model => model.Name, SpecificationFilterOperator.Equal, "Alice");
        }
    }

    private sealed class DisallowedFieldSpecification : Specification<TestModel>
    {
        public DisallowedFieldSpecification()
        {
            AllowField(model => model.Name);
            OrderBy(model => model.Age);
        }
    }

    private sealed class InvalidOffsetPaginationSpecification : Specification<TestModel>
    {
        public InvalidOffsetPaginationSpecification(int skip, int take)
        {
            Paginate(skip, take);
        }
    }

    private sealed class InvalidCursorPaginationSpecification : Specification<TestModel>
    {
        public InvalidCursorPaginationSpecification(int take)
        {
            Paginate("cursor", take);
        }
    }

    private sealed class NameEqualsSpecification : Specification<TestModel>
    {
        public NameEqualsSpecification(string name)
        {
            AllowField(model => model.Name);
            Where(model => model.Name, SpecificationFilterOperator.Equal, name);
        }
    }

    private sealed class AgeGreaterThanSpecification : Specification<TestModel>
    {
        public AgeGreaterThanSpecification(int age)
        {
            AllowField(model => model.Age);
            Where(model => model.Age, SpecificationFilterOperator.GreaterThan, age);
        }
    }

    private sealed class NameContainsSpecification : Specification<TestModel>
    {
        public NameContainsSpecification(string value)
        {
            AllowField(model => model.Name);
            Where(model => model.Name, SpecificationFilterOperator.Contains, value);
        }
    }

    private sealed class OrderedPagedAdultsSpecification : Specification<TestModel>
    {
        public OrderedPagedAdultsSpecification()
        {
            AllowField(model => model.Name);
            AllowField(model => model.Age);
            Where(model => model.Age, SpecificationFilterOperator.GreaterThanOrEqual, 30);
            OrderByDescending(model => model.Name);
            Paginate(1, 2);
        }
    }

    private sealed class NameProjectionSpecification : Specification<TestModel>
    {
        public NameProjectionSpecification()
        {
            AllowField(model => model.Name);
            AllowField(model => model.Age);
            Where(model => model.Age, SpecificationFilterOperator.GreaterThan, 20);
            OrderBy(model => model.Name);
            Select(model => model.Name);
        }
    }

    private sealed class CustomSpecification<TMember> : Specification<TestModel>
    {
        public CustomSpecification(
            SpecificationFilterOperator filterOperator,
            object? value,
            Expression<Func<TestModel, TMember>> fieldExpression)
        {
            AllowField(fieldExpression);
            Where(fieldExpression, filterOperator, value);
            OrderBy(fieldExpression);
        }
    }

    private sealed class InvalidOperatorSpecification : ISpecification<TestModel>
    {
        public IReadOnlyCollection<SpecificationFilter> Filters { get; } =
            [new(new SpecificationField("Age", "Age", typeof(int)), (SpecificationFilterOperator)999, 10)];

        public IReadOnlyCollection<SpecificationOrder> Orders { get; } = [];

        public SpecificationPagination? Pagination => null;

        public IReadOnlyCollection<string> AllowedFields { get; } = [];

        public SpecificationComposition Composition => SpecificationComposition.None;

        public IReadOnlyCollection<ISpecification<TestModel>> Children { get; } = [];

        public object? Projection => null;
    }

    private sealed class EmptyFieldPathSpecification : ISpecification<TestModel>
    {
        public IReadOnlyCollection<SpecificationFilter> Filters { get; } =
            [new(new SpecificationField("", "", typeof(string)), SpecificationFilterOperator.Equal, "Alice")];

        public IReadOnlyCollection<SpecificationOrder> Orders { get; } = [];

        public SpecificationPagination? Pagination => null;

        public IReadOnlyCollection<string> AllowedFields { get; } = [];

        public SpecificationComposition Composition => SpecificationComposition.None;

        public IReadOnlyCollection<ISpecification<TestModel>> Children { get; } = [];

        public object? Projection => null;
    }

    private sealed class NonStringContainsSpecification : ISpecification<TestModel>
    {
        public IReadOnlyCollection<SpecificationFilter> Filters { get; } =
            [new(new SpecificationField("Age", "Age", typeof(int)), SpecificationFilterOperator.Contains, 3)];

        public IReadOnlyCollection<SpecificationOrder> Orders { get; } = [];

        public SpecificationPagination? Pagination => null;

        public IReadOnlyCollection<string> AllowedFields { get; } = [];

        public SpecificationComposition Composition => SpecificationComposition.None;

        public IReadOnlyCollection<ISpecification<TestModel>> Children { get; } = [];

        public object? Projection => null;
    }

    private sealed class TestModel
    {
        public string Name { get; init; } = string.Empty;

        public int Age { get; init; }

        public Address Address { get; init; } = new();
    }

    private sealed class Address
    {
        public string City { get; init; } = string.Empty;
    }
}
