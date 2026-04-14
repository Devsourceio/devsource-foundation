using System.Linq.Expressions;
using DevSource.Foundation.Abstractions;
using DevSource.Stack.Abstractions;
using DevSource.Stack.Application;

namespace DevSource.Foundation.Application;

/// <summary>
/// Applies neutral specifications to <see cref="IQueryable{T}"/> sources using expression translation.
/// </summary>
/// <typeparam name="T">The source model type.</typeparam>
public sealed class SpecificationEvaluator<T> : ISpecificationEvaluator<T>
{
    /// <inheritdoc />
    public IQueryable<T> Evaluate(IQueryable<T> query, ISpecification<T> specification)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(specification);

        var filtered = ApplyPredicate(query, BuildPredicate(specification));
        var ordered = ApplyOrdering(filtered, specification.Orders);
        return ApplyPagination(ordered, specification.Pagination);
    }

    /// <inheritdoc />
    public IQueryable<TResult> Evaluate<TResult>(IQueryable<T> query, ISpecification<T> specification)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(specification);

        if (specification.Projection is not SpecificationProjection<T, TResult> projection)
        {
            throw new InvalidOperationException("The specification projection is missing or has an incompatible result type.");
        }

        var prepared = Evaluate(query, specification);
        return prepared.Select(projection.Selector);
    }

    private static IQueryable<T> ApplyPredicate(IQueryable<T> query, Expression<Func<T, bool>>? predicate)
    {
        return predicate is null ? query : query.Where(predicate);
    }

    private static IQueryable<T> ApplyOrdering(
        IQueryable<T> query,
        IReadOnlyCollection<SpecificationOrder> orders)
    {
        IOrderedQueryable<T>? ordered = null;

        foreach (var order in orders)
        {
            var keySelector = BuildKeySelector(order.Field.Path);

            ordered = ordered is null
                ? ApplyOrder(query, keySelector, order.Direction)
                : ApplyThenOrder(ordered, keySelector, order.Direction);
        }

        return ordered ?? query;
    }

    private static IQueryable<T> ApplyPagination(IQueryable<T> query, SpecificationPagination? pagination)
    {
        if (pagination is null)
        {
            return query;
        }

        var paged = query;

        if (pagination.Skip > 0)
        {
            paged = paged.Skip(pagination.Skip);
        }

        return paged.Take(pagination.Take);
    }

    private static Expression<Func<T, bool>>? BuildPredicate(ISpecification<T> specification)
    {
        Expression<Func<T, bool>>? ownPredicate = null;

        foreach (var filter in specification.Filters)
        {
            var filterPredicate = BuildFilterPredicate(filter);
            ownPredicate = ownPredicate is null
                ? filterPredicate
                : CombineWithAnd(ownPredicate, filterPredicate);
        }

        if (specification.Composition == SpecificationComposition.None)
        {
            return ownPredicate;
        }

        var childPredicates = specification.Children
            .Select(BuildPredicate)
            .Where(predicate => predicate is not null)
            .Cast<Expression<Func<T, bool>>>()
            .ToArray();

        if (childPredicates.Length == 0)
        {
            return ownPredicate;
        }

        var composed = childPredicates[0];

        for (var i = 1; i < childPredicates.Length; i++)
        {
            composed = specification.Composition == SpecificationComposition.Or
                ? CombineWithOr(composed, childPredicates[i])
                : CombineWithAnd(composed, childPredicates[i]);
        }

        if (specification.Composition == SpecificationComposition.Not)
        {
            composed = Negate(composed);
        }

        return ownPredicate is null ? composed : CombineWithAnd(ownPredicate, composed);
    }

    private static Expression<Func<T, bool>> BuildFilterPredicate(SpecificationFilter filter)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var member = BuildMemberAccess(parameter, filter.Field.Path);
        var memberType = member.Type;
        var underlyingType = Nullable.GetUnderlyingType(memberType) ?? memberType;

        if ((filter.Operator is SpecificationFilterOperator.Contains or
             SpecificationFilterOperator.StartsWith or
             SpecificationFilterOperator.EndsWith) && underlyingType != typeof(string))
        {
            throw new InvalidOperationException($"Operator '{filter.Operator}' requires a string field.");
        }

        Expression body;

        if (filter.Operator is SpecificationFilterOperator.Contains or
            SpecificationFilterOperator.StartsWith or
            SpecificationFilterOperator.EndsWith)
        {
            var constant = Expression.Constant(filter.Value?.ToString(), typeof(string));
            var methodName = filter.Operator switch
            {
                SpecificationFilterOperator.Contains => nameof(string.Contains),
                SpecificationFilterOperator.StartsWith => nameof(string.StartsWith),
                _ => nameof(string.EndsWith),
            };

            body = Expression.Call(member, methodName, Type.EmptyTypes, constant);
        }
        else
        {
            var constantValue = filter.Value is null
                ? null
                : Convert.ChangeType(filter.Value, underlyingType);

            Expression right = Expression.Constant(constantValue, underlyingType);

            if (memberType != underlyingType)
            {
                right = Expression.Convert(right, memberType);
            }

            body = filter.Operator switch
            {
                SpecificationFilterOperator.Equal => Expression.Equal(member, right),
                SpecificationFilterOperator.NotEqual => Expression.NotEqual(member, right),
                SpecificationFilterOperator.GreaterThan => Expression.GreaterThan(member, right),
                SpecificationFilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, right),
                SpecificationFilterOperator.LessThan => Expression.LessThan(member, right),
                SpecificationFilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(member, right),
                _ => throw new InvalidOperationException($"Unsupported operator '{filter.Operator}'."),
            };
        }

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression BuildMemberAccess(Expression parameter, string path)
    {
        var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            throw new InvalidOperationException("Field path cannot be empty.");
        }

        Expression current = parameter;

        foreach (var segment in segments)
        {
            current = Expression.PropertyOrField(current, segment);
        }

        return current;
    }

    private static LambdaExpression BuildKeySelector(string path)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var body = BuildMemberAccess(parameter, path);
        var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), body.Type);
        return Expression.Lambda(delegateType, body, parameter);
    }

    private static IOrderedQueryable<T> ApplyOrder(
        IQueryable<T> source,
        LambdaExpression keySelector,
        SpecificationOrderDirection direction)
    {
        var methodName = direction == SpecificationOrderDirection.Ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);
        return (IOrderedQueryable<T>)InvokeQueryable(source, methodName, keySelector);
    }

    private static IOrderedQueryable<T> ApplyThenOrder(
        IOrderedQueryable<T> source,
        LambdaExpression keySelector,
        SpecificationOrderDirection direction)
    {
        var methodName = direction == SpecificationOrderDirection.Ascending ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending);
        return (IOrderedQueryable<T>)InvokeQueryable(source, methodName, keySelector);
    }

    private static object InvokeQueryable(IQueryable<T> source, string methodName, LambdaExpression keySelector)
    {
        var method = typeof(Queryable)
            .GetMethods()
            .Single(m =>
                m.Name == methodName &&
                m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), keySelector.ReturnType);

        return method.Invoke(null, [source, keySelector])
               ?? throw new InvalidOperationException($"Could not invoke query method '{methodName}'.");
    }

    private static Expression<Func<T, bool>> CombineWithAnd(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var body = Expression.AndAlso(
            ReplaceParameter(left.Body, left.Parameters[0], parameter),
            ReplaceParameter(right.Body, right.Parameters[0], parameter));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression<Func<T, bool>> CombineWithOr(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var body = Expression.OrElse(
            ReplaceParameter(left.Body, left.Parameters[0], parameter),
            ReplaceParameter(right.Body, right.Parameters[0], parameter));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression<Func<T, bool>> Negate(Expression<Func<T, bool>> predicate)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var body = Expression.Not(ReplaceParameter(predicate.Body, predicate.Parameters[0], parameter));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression ReplaceParameter(Expression body, ParameterExpression source, ParameterExpression target)
    {
        return new ParameterReplaceVisitor(source, target).Visit(body)
               ?? throw new InvalidOperationException("Failed to replace expression parameter.");
    }

    private sealed class ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == source ? target : base.VisitParameter(node);
        }
    }
}
