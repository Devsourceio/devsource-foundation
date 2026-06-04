namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Represents a sort instruction in the neutral specification model.
/// </summary>
public sealed class SpecificationOrder
{
    /// <summary>
    /// Gets the target field.
    /// </summary>
    public SpecificationField Field { get; }

    /// <summary>
    /// Gets the sort direction.
    /// </summary>
    public SpecificationOrderDirection Direction { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationOrder"/> class.
    /// </summary>
    /// <param name="field">Target field.</param>
    /// <param name="direction">Sort direction.</param>
    public SpecificationOrder(SpecificationField field, SpecificationOrderDirection direction)
    {
        Field = field;
        Direction = direction;
    }    
}