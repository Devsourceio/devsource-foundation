namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Defines a provider evaluator contract that translates neutral specifications into queryable operations.
/// </summary>
/// <typeparam name="T">The source model type.</typeparam>
public interface ISpecificationEvaluator<T>
{
    /// <summary>
    /// Applies specification filters, ordering, and pagination to a query source.
    /// </summary>
    /// <param name="query">Query source.</param>
    /// <param name="specification">Specification to evaluate.</param>
    /// <returns>A query source with specification rules applied.</returns>
    IQueryable<T> Evaluate(IQueryable<T> query, ISpecification<T> specification);

    /// <summary>
    /// Applies specification filters, ordering, pagination, and projection to a query source.
    /// </summary>
    /// <typeparam name="TResult">The projected result type.</typeparam>
    /// <param name="query">Query source.</param>
    /// <param name="specification">Specification to evaluate.</param>
    /// <returns>A projected query source.</returns>
    IQueryable<TResult> Evaluate<TResult>(IQueryable<T> query, ISpecification<T> specification);
}
