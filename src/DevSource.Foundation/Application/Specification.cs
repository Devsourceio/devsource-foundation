using System.Collections.ObjectModel;
using System.Linq.Expressions;
using DevSource.Foundation.Abstractions;

namespace DevSource.Foundation.Application;

/// <summary>
/// Provides a base implementation for provider-agnostic query specifications.
/// </summary>
/// <typeparam name="T">The source model type.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    private readonly List<SpecificationFilter> _filters = [];
    private readonly List<SpecificationOrder> _orders = [];
    private readonly HashSet<string> _allowedFields = new(StringComparer.Ordinal);
    private readonly List<string> _allowedFieldList = [];
    private readonly ReadOnlyCollection<SpecificationFilter> _filtersView;
    private readonly ReadOnlyCollection<SpecificationOrder> _ordersView;
    private readonly ReadOnlyCollection<string> _allowedFieldsView;

    /// <summary>
    /// Initializes an empty specification.
    /// </summary>
    protected Specification()
    {
        _filtersView = new ReadOnlyCollection<SpecificationFilter>(_filters);
        _ordersView = new ReadOnlyCollection<SpecificationOrder>(_orders);
        _allowedFieldsView = new ReadOnlyCollection<string>(_allowedFieldList);
    }

    /// <summary>
    /// Gets specification filters.
    /// </summary>
    public IReadOnlyCollection<SpecificationFilter> Filters => _filtersView;

    /// <summary>
    /// Gets specification sorting instructions.
    /// </summary>
    public IReadOnlyCollection<SpecificationOrder> Orders => _ordersView;

    /// <summary>
    /// Gets optional pagination settings.
    /// </summary>
    public SpecificationPagination? Pagination { get; private set; }

    /// <summary>
    /// Gets the field whitelist used for filter and order validation.
    /// </summary>
    public IReadOnlyCollection<string> AllowedFields => _allowedFieldsView;

    /// <summary>
    /// Gets the composition mode.
    /// </summary>
    public virtual SpecificationComposition Composition => SpecificationComposition.None;

    /// <summary>
    /// Gets nested child specifications.
    /// </summary>
    public virtual IReadOnlyCollection<ISpecification<T>> Children => Array.Empty<ISpecification<T>>();

    /// <summary>
    /// Gets optional projection descriptor.
    /// </summary>
    public object? Projection { get; private set; }

    /// <summary>
    /// Allows a field in the specification whitelist.
    /// </summary>
    /// <typeparam name="TMember">The field type.</typeparam>
    /// <param name="fieldExpression">Field expression.</param>
    protected void AllowField<TMember>(Expression<Func<T, TMember>> fieldExpression)
    {
        var field = SpecificationFieldResolver.Resolve(fieldExpression);
        if (_allowedFields.Add(field.Path))
        {
            _allowedFieldList.Add(field.Path);
        }
    }

    /// <summary>
    /// Adds a filter instruction.
    /// </summary>
    /// <typeparam name="TMember">The field type.</typeparam>
    /// <param name="fieldExpression">Field expression.</param>
    /// <param name="filterOperator">Filter operator.</param>
    /// <param name="value">Filter value.</param>
    protected void Where<TMember>(
        Expression<Func<T, TMember>> fieldExpression,
        SpecificationFilterOperator filterOperator,
        object? value)
    {
        var field = SpecificationFieldResolver.Resolve(fieldExpression);
        EnsureAllowed(field);
        _filters.Add(new SpecificationFilter(field, filterOperator, value));
    }

    /// <summary>
    /// Adds an ascending sort instruction.
    /// </summary>
    /// <typeparam name="TMember">The field type.</typeparam>
    /// <param name="fieldExpression">Field expression.</param>
    protected void OrderBy<TMember>(Expression<Func<T, TMember>> fieldExpression)
    {
        var field = SpecificationFieldResolver.Resolve(fieldExpression);
        EnsureAllowed(field);
        _orders.Add(new SpecificationOrder(field, SpecificationOrderDirection.Ascending));
    }

    /// <summary>
    /// Adds a descending sort instruction.
    /// </summary>
    /// <typeparam name="TMember">The field type.</typeparam>
    /// <param name="fieldExpression">Field expression.</param>
    protected void OrderByDescending<TMember>(Expression<Func<T, TMember>> fieldExpression)
    {
        var field = SpecificationFieldResolver.Resolve(fieldExpression);
        EnsureAllowed(field);
        _orders.Add(new SpecificationOrder(field, SpecificationOrderDirection.Descending));
    }

    /// <summary>
    /// Sets offset-based pagination.
    /// </summary>
    /// <param name="skip">Number of items to skip.</param>
    /// <param name="take">Number of items to take.</param>
    protected void Paginate(int skip, int take)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);
        Pagination = new SpecificationPagination(skip, take);
    }

    /// <summary>
    /// Sets cursor-based pagination with page size.
    /// </summary>
    /// <param name="cursor">Cursor token.</param>
    /// <param name="take">Number of items to take.</param>
    protected void Paginate(string cursor, int take)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);
        Pagination = new SpecificationPagination(0, take, cursor);
    }

    /// <summary>
    /// Sets a typed projection selector.
    /// </summary>
    /// <typeparam name="TResult">Projection result type.</typeparam>
    /// <param name="selector">Projection selector expression.</param>
    protected void Select<TResult>(Expression<Func<T, TResult>> selector)
    {
        Projection = new SpecificationProjection<T, TResult>(selector);
    }

    /// <summary>
    /// Composes this specification with another specification using conjunction.
    /// </summary>
    /// <param name="other">The other specification.</param>
    /// <returns>A composed specification.</returns>
    public ISpecification<T> And(ISpecification<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new CompositeSpecification<T>(SpecificationComposition.And, this, other);
    }

    /// <summary>
    /// Composes this specification with another specification using disjunction.
    /// </summary>
    /// <param name="other">The other specification.</param>
    /// <returns>A composed specification.</returns>
    public ISpecification<T> Or(ISpecification<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new CompositeSpecification<T>(SpecificationComposition.Or, this, other);
    }

    /// <summary>
    /// Negates this specification.
    /// </summary>
    /// <returns>A composed specification.</returns>
    public ISpecification<T> Not()
    {
        return new CompositeSpecification<T>(SpecificationComposition.Not, this);
    }

    private void EnsureAllowed(SpecificationField field)
    {
        if (_allowedFields.Count == 0)
        {
            throw new InvalidOperationException("At least one allowed field must be declared before using filters or ordering.");
        }

        if (!_allowedFields.Contains(field.Path))
        {
            throw new InvalidOperationException($"Field '{field.Path}' is not allowed by this specification whitelist.");
        }
    }
}
