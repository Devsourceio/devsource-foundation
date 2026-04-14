namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Represents supported filter operators.
/// </summary>
public enum SpecificationFilterOperator
{
    /// <summary>
    /// Applies equality comparison.
    /// </summary>
    Equal,

    /// <summary>
    /// Applies inequality comparison.
    /// </summary>
    NotEqual,

    /// <summary>
    /// Applies greater-than comparison.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Applies greater-than-or-equal comparison.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Applies less-than comparison.
    /// </summary>
    LessThan,

    /// <summary>
    /// Applies less-than-or-equal comparison.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Applies string contains comparison.
    /// </summary>
    Contains,

    /// <summary>
    /// Applies string starts-with comparison.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Applies string ends-with comparison.
    /// </summary>
    EndsWith,
}