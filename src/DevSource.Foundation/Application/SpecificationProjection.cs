using System.Linq.Expressions;

namespace DevSource.Foundation.Application;

/// <summary>
/// Represents a typed projection descriptor for specifications.
/// </summary>
/// <typeparam name="TSource">The source model type.</typeparam>
/// <typeparam name="TResult">The projection result type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="SpecificationProjection{TSource, TResult}"/> class.
/// </remarks>
/// <param name="selector">Projection selector expression.</param>
public sealed class SpecificationProjection<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
{
    /// <summary>
    /// Gets the projection selector expression.
    /// </summary>
    public Expression<Func<TSource, TResult>> Selector { get; } = selector ?? throw new ArgumentNullException(nameof(selector));
}
