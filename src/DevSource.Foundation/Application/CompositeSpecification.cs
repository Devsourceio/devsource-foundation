using DevSource.Foundation.Abstractions;

namespace DevSource.Foundation.Application;

/// <summary>
/// Represents a composed specification (AND, OR, NOT) built from child specifications.
/// </summary>
/// <typeparam name="T">The source model type.</typeparam>
public sealed class CompositeSpecification<T> : ISpecification<T>
{
    private readonly IReadOnlyCollection<ISpecification<T>> children;

    /// <inheritdoc />
    public IReadOnlyCollection<SpecificationFilter> Filters => [];

    /// <inheritdoc />
    public IReadOnlyCollection<SpecificationOrder> Orders => [];

    /// <inheritdoc />
    public SpecificationPagination? Pagination => null;

    /// <inheritdoc />
    public IReadOnlyCollection<string> AllowedFields => [];

    /// <inheritdoc />
    public SpecificationComposition Composition { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<ISpecification<T>> Children => children;

    /// <inheritdoc />
    public object? Projection => null;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeSpecification{T}"/> class.
    /// </summary>
    /// <param name="composition">Composition mode.</param>
    /// <param name="children">Child specifications.</param>
    public CompositeSpecification(SpecificationComposition composition, params ISpecification<T>[] children)
    {
        if (composition == SpecificationComposition.None)
        {
            throw new ArgumentException("Composite specification must use a composition mode.", nameof(composition));
        }

        if (children is null || children.Length == 0)
        {
            throw new ArgumentException("Composite specification requires at least one child.", nameof(children));
        }

        if (composition == SpecificationComposition.Not && children.Length != 1)
        {
            throw new ArgumentException("NOT composition requires exactly one child.", nameof(children));
        }

        Composition = composition;
        this.children = Array.AsReadOnly(children);
    }    
}
