# DevSource Foundation

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C%23](https://img.shields.io/badge/C%23-13-239120?logo=csharp)](https://learn.microsoft.com/dotnet/csharp/)
[![NuGet](https://img.shields.io/badge/NuGet-not%20published%20yet-004880?logo=nuget)](https://www.nuget.org/)
[![Tests](https://img.shields.io/badge/tests-68%20passing-brightgreen)](#running-tests)

DevSource Foundation is a lightweight .NET foundation library for building cloud-native applications with clear architectural boundaries. It provides reusable primitives, domain building blocks, provider-agnostic repository contracts, and an extensible specification system for filtering, sorting, pagination, projection, and query composition.

The library is organized around four layers:

- `Primitives`: result handling, errors, optional values, and guard clauses.
- `Domain`: entities, value objects, domain events, and aggregate roots.
- `Abstractions`: repository, cache, event bus, unit of work, and specification contracts.
- `Application`: application service base class and specification implementation/evaluation.

## Why DevSource Foundation?

Use this library when you want to:

- keep domain code free from infrastructure concerns
- model expected failures with `Result` instead of exceptions
- represent optional values without null-driven flow
- enforce repository contracts that do not leak `IQueryable`
- build reusable, strongly typed query specifications
- compose filters with `AND`, `OR`, and `NOT`
- keep application services focused on orchestration and transaction boundaries

## Target Framework

- `net10.0`

## Installation

The package is not currently published to NuGet. You can consume it from source today.

### Option 1: Project reference

```xml
<ItemGroup>
  <ProjectReference Include="..\src\DevSource.Foundation\DevSource.Foundation.csproj" />
</ItemGroup>
```

### Option 2: NuGet package

When the package is published, install it with:

```bash
dotnet add package DevSource.Foundation
```

## Running Tests

```bash
dotnet test
```

The current test suite covers:

- primitives
- domain building blocks
- abstractions contracts
- application services
- specification building and evaluation

## Features

### 1. Errors

`Error` represents a stable error code plus a human-readable message.

```csharp
using DevSource.Foundation.Primitives;

var error = new Error("customer.not_found", "Customer was not found.");

Console.WriteLine(error.Code);
Console.WriteLine(error.Message);
Console.WriteLine(error); // customer.not_found: Customer was not found.
```

Notes:

- equality is based on `Code`
- `Error.None` represents the absence of an error
- invalid code or message throws `ArgumentException`

### 2. Result Pattern

Use `Result` for expected and recoverable failures.

```csharp
using DevSource.Foundation.Primitives;

Result success = Result.Success();
Result failure = Result.Failure(
    new Error("validation.required", "Name is required."),
    new Error("validation.invalid_age", "Age must be greater than zero."));

if (failure.IsFailure)
{
    foreach (var item in failure.Errors)
    {
        Console.WriteLine(item);
    }
}
```

Typed results expose a value on success:

```csharp
using DevSource.Foundation.Primitives;

Result<Guid> created = Result<Guid>.Success(Guid.NewGuid());

if (created.IsSuccess)
{
    Console.WriteLine(created.Value);
}
```

Behavior:

- success results cannot contain errors
- failure results must contain at least one error
- duplicate errors are normalized with `Distinct()`
- accessing `Value` on a failed `Result<T>` throws `InvalidOperationException`

### 3. Guard Clauses

Use `Guard` for programmer errors and boundary validation that should fail fast.

```csharp
using DevSource.Foundation.Primitives;

var id = Guard.NotDefault(Guid.NewGuid(), nameof(id));
var name = Guard.NotEmpty("Alice", nameof(name));
var service = Guard.NotNull(new object(), nameof(service));
```

Available guards:

- `Guard.NotNull`
- `Guard.NotEmpty`
- `Guard.NotDefault`

### 4. Maybe

`Maybe<T>` models an optional value without relying on nullable flow as the primary API.

```csharp
using DevSource.Foundation.Primitives;

Maybe<string> maybeName = Maybe.From("Alice");
Maybe<string> none = Maybe.None;

if (maybeName.HasValue)
{
    Console.WriteLine(maybeName.Value);
}

Console.WriteLine(none.HasValue); // False
```

Behavior:

- `Maybe.From(value)` rejects `null`
- `Maybe.None` creates an empty value
- accessing `Value` when `HasValue` is `false` throws `InvalidOperationException`

### 5. Entity

`Entity<TId>` gives identity-based equality.

```csharp
using DevSource.Foundation.Domain;

public sealed class Customer : Entity<Guid>
{
    public Customer(Guid id)
        : base(id)
    {
    }
}

var id = Guid.NewGuid();
var left = new Customer(id);
var right = new Customer(id);

Console.WriteLine(left == right); // True
```

Behavior:

- equality requires the same runtime type
- transient entities with default identifiers are not equal by identity
- hash code is identity-based for persisted entities

### 6. Value Object

`ValueObject` gives structural equality based on atomic components.

```csharp
using DevSource.Foundation.Domain;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

Behavior:

- equality is structural
- type differences matter even if components match
- hash codes are built from equality components

### 7. Domain Events and Aggregate Roots

`DomainEvent` captures an immutable event with `EventId` and `OccurredOnUtc`.

```csharp
using DevSource.Foundation.Domain;

public sealed class OrderPlacedDomainEvent(Guid orderId) : DomainEvent
{
    public Guid OrderId { get; } = orderId;
}
```

`AggregateRoot<TId>` stores domain events until the application layer commits and publishes them.

```csharp
using DevSource.Foundation.Domain;

public sealed class Order : AggregateRoot<Guid>
{
    public Order(Guid id)
        : base(id)
    {
    }

    public void Place()
    {
        AddDomainEvent(new OrderPlacedDomainEvent(Id));
    }
}
```

Behavior:

- events are stored in `DomainEvents`
- aggregates can clear events with `ClearDomainEvents()`
- `AddDomainEvent` rejects `null`
- publication is intentionally outside the domain model

### 8. Repository Abstractions

The library defines contracts without tying you to any ORM or query provider.

```csharp
using DevSource.Foundation.Abstractions;

public interface ICustomerRepository : IRepository<Customer, Guid, CustomerReadModel>
{
}
```

Available contracts:

- `IReadRepository<TReadModel>`
- `IWriteRepository<TWriteModel, TId>`
- `IWriteRepository<TWriteModel>`
- `IRepository<TWriteModel, TId, TReadModel>`
- `IRepository<TModel, TId>`
- `IRepository<TModel>`

Important detail:

- repository contracts do not expose `IQueryable` or raw expressions
- complex queries should be represented through specifications

### 9. Unit of Work, Cache, and Event Bus Contracts

These contracts support application orchestration without forcing a specific infrastructure implementation.

```csharp
using DevSource.Foundation.Abstractions;

public sealed class CheckoutService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly ICache cache;
    private readonly IEventBus eventBus;

    public CheckoutService(IUnitOfWork unitOfWork, ICache cache, IEventBus eventBus)
    {
        this.unitOfWork = unitOfWork;
        this.cache = cache;
        this.eventBus = eventBus;
    }
}
```

Provided contracts:

- `IUnitOfWork`
- `ICache`
- `IEventBus`

### 10. Application Services

`ApplicationService` is a small base class for use-case orchestration around `IUnitOfWork`.

```csharp
using DevSource.Foundation.Abstractions;
using DevSource.Foundation.Application;
using DevSource.Foundation.Primitives;

public sealed class CreateCustomerService : ApplicationService
{
    private readonly IWriteRepository<Customer, Guid> repository;

    public CreateCustomerService(
        IWriteRepository<Customer, Guid> repository,
        IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
        this.repository = repository;
    }

    public Task<Result<Guid>> ExecuteAsync(string name, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(async token =>
        {
            var customer = new Customer(Guid.NewGuid(), name);
            await repository.AddAsync(customer, token);
            return Result<Guid>.Success(customer.Id);
        }, cancellationToken);
    }
}

public sealed class Customer
{
    public Customer(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; }
    public string Name { get; }
}
```

Behavior:

- `CommitAsync()` delegates to `IUnitOfWork`
- `ExecuteAsync(...)` commits after the operation succeeds
- failed operations do not commit

### 11. User, Time, and Tenant Context Contracts

The application layer also exposes simple context abstractions:

- `ICurrentUser`
- `IDateTimeProvider`
- `ITenantProvider`

These are useful when your use cases need identity, time, or tenant context without depending on ASP.NET Core or another framework.

## Specifications

The specification system is the most expressive part of the library. It lets you define provider-agnostic queries using strongly typed member access.

### What a specification can do

- whitelist allowed fields
- add filters
- add ordering
- add pagination
- add projection
- compose child specifications with `And`, `Or`, and `Not`

### Supported filter operators

- `Equal`
- `NotEqual`
- `GreaterThan`
- `GreaterThanOrEqual`
- `LessThan`
- `LessThanOrEqual`
- `Contains`
- `StartsWith`
- `EndsWith`

### Building a specification

```csharp
using DevSource.Foundation.Abstractions;
using DevSource.Foundation.Application;

public sealed class ActiveAdultCustomersSpecification : Specification<CustomerReadModel>
{
    public ActiveAdultCustomersSpecification()
    {
        AllowField(x => x.Name);
        AllowField(x => x.Age);

        Where(x => x.Age, SpecificationFilterOperator.GreaterThanOrEqual, 18);
        OrderBy(x => x.Name);
        Paginate(0, 20);
    }
}

public sealed class CustomerReadModel
{
    public string Name { get; init; } = string.Empty;
    public int Age { get; init; }
}
```

### Projection

```csharp
using DevSource.Foundation.Abstractions;
using DevSource.Foundation.Application;

public sealed class CustomerNamesSpecification : Specification<CustomerReadModel>
{
    public CustomerNamesSpecification()
    {
        AllowField(x => x.Name);
        AllowField(x => x.Age);

        Where(x => x.Age, SpecificationFilterOperator.GreaterThan, 20);
        OrderBy(x => x.Name);
        Select(x => x.Name);
    }
}
```

### Composition

```csharp
var adults = new AgeSpecification(18);
var namedWithA = new NameContainsSpecification("a");

var andSpecification = adults.And(namedWithA);
var orSpecification = adults.Or(namedWithA);
var notSpecification = namedWithA.Not();
```

### Evaluating a specification

`SpecificationEvaluator<T>` translates the neutral model into `IQueryable<T>` operations.

```csharp
using DevSource.Foundation.Application;

var data = new List<CustomerReadModel>
{
    new() { Name = "Alice", Age = 30 },
    new() { Name = "Bruno", Age = 20 },
    new() { Name = "Carla", Age = 35 }
}.AsQueryable();

var evaluator = new SpecificationEvaluator<CustomerReadModel>();
var specification = new CustomerNamesSpecification();

string[] names = evaluator.Evaluate<string>(data, specification).ToArray();
```

### Security and correctness details

- fields must be explicitly allowed with `AllowField(...)`
- ordering and filtering on a non-whitelisted field throws `InvalidOperationException`
- string operators only work on string fields
- unsupported expression shapes are rejected
- empty field paths are rejected

## Example Architecture Flow

1. Model your core concepts with `Entity`, `ValueObject`, and `AggregateRoot`.
2. Return `Result` for expected business outcomes.
3. Use `Guard` for programmer errors and invalid method usage.
4. Define repository contracts through `IRepository`, `IReadRepository`, and `IWriteRepository`.
5. Represent read-side query rules with `Specification<T>`.
6. Use `ApplicationService` to orchestrate use cases and commit through `IUnitOfWork`.
7. Publish domain or integration events outside the domain layer.

## Design Principles Reflected in the Code

- provider-agnostic abstractions
- layered architecture with clear boundaries
- no repository leakage of `IQueryable`
- specification-based querying for complex reads
- fail-fast guards for invalid API usage
- result-based handling for expected failures

## Project Structure

```text
src/
  DevSource.Foundation/
    Primitives/
    Domain/
    Abstractions/
    Application/

tests/
  DevSource.Foundation.Tests/
```

## Development Notes

- The source library targets `net10.0`.
- The current repository includes xUnit tests for the core API surface.
- The NuGet badge is informational for now because the package is not yet published.
- The tests badge is static until a CI workflow is added to the repository.

## License

This repository does not currently declare a license file.
