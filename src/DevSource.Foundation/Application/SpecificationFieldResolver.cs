using System.Collections.Concurrent;
using System.Linq.Expressions;
using DevSource.Foundation.Abstractions;

namespace DevSource.Foundation.Application;

/// <summary>
/// Resolves strongly-typed member expressions into neutral specification fields.
/// </summary>
public static class SpecificationFieldResolver
{
    private static readonly ConcurrentDictionary<string, SpecificationField> Cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Resolves a member expression to a <see cref="SpecificationField"/> descriptor.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="TMember">The member type.</typeparam>
    /// <param name="expression">Member access expression.</param>
    /// <returns>The resolved field descriptor.</returns>
    public static SpecificationField Resolve<T, TMember>(Expression<Func<T, TMember>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var path = ResolvePath(expression.Body);
        var cacheKey = $"{typeof(T).FullName}|{path}";

        return Cache.GetOrAdd(cacheKey, _ =>
        {
            var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var name = segments[^1];
            return new SpecificationField(name, path, typeof(TMember));
        });
    }

    /// <summary>
    /// Resolves a member expression to a dot-separated property path.
    /// </summary>
    /// <param name="expressionBody">Expression body.</param>
    /// <returns>The resolved property path.</returns>
    public static string ResolvePath(Expression expressionBody)
    {
        var body = UnwrapConvert(expressionBody);

        var members = new Stack<string>();
        var current = body;

        while (current is MemberExpression member)
        {
            members.Push(member.Member.Name);
            current = UnwrapConvert(member.Expression);
        }

        if (current is not ParameterExpression || members.Count == 0)
        {
            throw new ArgumentException("Only direct member access expressions are supported.", nameof(expressionBody));
        }

        return string.Join('.', members);
    }

    private static Expression UnwrapConvert(Expression? expression)
    {
        while (expression is UnaryExpression unary &&
               (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
        {
            expression = unary.Operand;
        }

        return expression ?? throw new ArgumentException("Expression cannot be null.", nameof(expression));
    }
}
