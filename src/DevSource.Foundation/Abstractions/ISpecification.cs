using DevSource.Stack.Abstractions;

namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Represents a provider-agnostic query specification contract.
/// </summary>
/// <typeparam name="T">The target model type.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the specification filters.
    /// </summary>
    IReadOnlyCollection<SpecificationFilter> Filters { get; }

    /// <summary>
    /// Gets the specification sorting instructions.
    /// </summary>
    IReadOnlyCollection<SpecificationOrder> Orders { get; }

    /// <summary>
    /// Gets optional pagination settings.
    /// </summary>
    SpecificationPagination? Pagination { get; }

    /// <summary>
    /// Gets the field whitelist used for filter and order validation.
    /// </summary>
    IReadOnlyCollection<string> AllowedFields { get; }

    /// <summary>
    /// Gets the specification composition mode.
    /// </summary>
    SpecificationComposition Composition { get; }

    /// <summary>
    /// Gets nested specifications used in composition.
    /// </summary>
    IReadOnlyCollection<ISpecification<T>> Children { get; }

    /// <summary>
    /// Gets the optional projection descriptor.
    /// </summary>
    object? Projection { get; }
}
