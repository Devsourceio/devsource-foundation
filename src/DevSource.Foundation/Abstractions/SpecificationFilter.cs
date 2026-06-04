namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Represents a filter operation in the neutral specification model.
/// </summary>
public sealed class SpecificationFilter
{
    /// <summary>
    /// Gets the target field.
    /// </summary>
    public SpecificationField Field { get; }

    /// <summary>
    /// Gets the filter operator.
    /// </summary>
    public SpecificationFilterOperator Operator { get; }

    /// <summary>
    /// Gets the filter value.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationFilter"/> class.
    /// </summary>
    /// <param name="field">Target field.</param>
    /// <param name="filterOperator">Filter operator.</param>
    /// <param name="value">Filter value.</param>
    public SpecificationFilter(SpecificationField field, SpecificationFilterOperator filterOperator, object? value)
    {
        Field = field;
        Operator = filterOperator;
        Value = value;
    }
}