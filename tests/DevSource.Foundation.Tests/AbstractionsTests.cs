using DevSource.Foundation.Abstractions;

namespace DevSource.Foundation.Tests;

public sealed class AbstractionsTests
{
    [Fact]
    public void SpecificationModels_Constructors_AssignProperties()
    {
        // Arrange
        var field = new SpecificationField("Name", "Customer.Name", typeof(string));

        // Act
        var filter = new SpecificationFilter(field, SpecificationFilterOperator.Contains, "devsource");
        var order = new SpecificationOrder(field, SpecificationOrderDirection.Descending);
        var pagination = new SpecificationPagination(5, 10, "cursor-1");

        // Assert
        Assert.Equal("Name", field.Name);
        Assert.Equal("Customer.Name", field.Path);
        Assert.Equal(typeof(string), field.Type);
        Assert.Same(field, filter.Field);
        Assert.Equal(SpecificationFilterOperator.Contains, filter.Operator);
        Assert.Equal("devsource", filter.Value);
        Assert.Same(field, order.Field);
        Assert.Equal(SpecificationOrderDirection.Descending, order.Direction);
        Assert.Equal(5, pagination.Skip);
        Assert.Equal(10, pagination.Take);
        Assert.Equal("cursor-1", pagination.Cursor);
    }

    [Fact]
    public void SpecificationEnums_ExposeExpectedValues()
    {
        // Act
        var filterOperators = Enum.GetValues<SpecificationFilterOperator>();
        var compositions = Enum.GetValues<SpecificationComposition>();
        var directions = Enum.GetValues<SpecificationOrderDirection>();

        // Assert
        Assert.Contains(SpecificationFilterOperator.Equal, filterOperators);
        Assert.Contains(SpecificationFilterOperator.EndsWith, filterOperators);
        Assert.Contains(SpecificationComposition.None, compositions);
        Assert.Contains(SpecificationComposition.Not, compositions);
        Assert.Contains(SpecificationOrderDirection.Ascending, directions);
        Assert.Contains(SpecificationOrderDirection.Descending, directions);
    }

    [Fact]
    public void RepositoryInterfaces_ExposeExpectedInheritanceContracts()
    {
        // Arrange
        var repositoryType = typeof(IRepository<,>);
        var repositoryWithReadType = typeof(IRepository<,,>);
        var writeRepositoryType = typeof(IWriteRepository<,>);
        var readRepositoryType = typeof(IReadRepository<>);

        // Act
        var repositoryInterfaces = repositoryType.GetInterfaces();
        var repositoryWithReadInterfaces = repositoryWithReadType.GetInterfaces();
        var writeRepositoryInterfaces = typeof(IWriteRepository<>).GetInterfaces();

        // Assert
        Assert.Contains(repositoryInterfaces, type => type.IsGenericType && type.GetGenericTypeDefinition() == repositoryWithReadType);
        Assert.Contains(repositoryWithReadInterfaces, type => type.IsGenericType && type.GetGenericTypeDefinition() == writeRepositoryType);
        Assert.Contains(repositoryWithReadInterfaces, type => type.IsGenericType && type.GetGenericTypeDefinition() == readRepositoryType);
        Assert.Contains(writeRepositoryInterfaces, type => type.IsGenericType && type.GetGenericTypeDefinition() == writeRepositoryType);
    }

    [Fact]
    public void Contracts_ExposeExpectedMembers()
    {
        // Arrange
        var specificationType = typeof(ISpecification<TestModel>);
        var evaluatorType = typeof(ISpecificationEvaluator<TestModel>);
        var cacheType = typeof(ICache);
        var eventBusType = typeof(IEventBus);
        var unitOfWorkType = typeof(IUnitOfWork);

        // Act
        var specificationProperties = specificationType.GetProperties().Select(property => property.Name).ToArray();
        var evaluatorMethods = evaluatorType.GetMethods().Select(method => method.Name).ToArray();
        var cacheMethods = cacheType.GetMethods().Select(method => method.Name).ToArray();
        var eventBusMethod = eventBusType.GetMethod(nameof(IEventBus.PublishAsync));
        var commitMethod = unitOfWorkType.GetMethod(nameof(IUnitOfWork.CommitAsync));

        // Assert
        Assert.Contains("Filters", specificationProperties);
        Assert.Contains("Orders", specificationProperties);
        Assert.Contains("Pagination", specificationProperties);
        Assert.Contains("AllowedFields", specificationProperties);
        Assert.Contains("Composition", specificationProperties);
        Assert.Contains("Children", specificationProperties);
        Assert.Contains("Projection", specificationProperties);
        Assert.Equal(2, evaluatorMethods.Count(name => name == nameof(ISpecificationEvaluator<TestModel>.Evaluate)));
        Assert.Contains(nameof(ICache.GetAsync), cacheMethods);
        Assert.Contains(nameof(ICache.SetAsync), cacheMethods);
        Assert.Contains(nameof(ICache.RemoveAsync), cacheMethods);
        Assert.NotNull(eventBusMethod);
        Assert.True(eventBusMethod!.IsGenericMethod);
        Assert.NotNull(commitMethod);
    }

    private sealed class TestModel;
}
