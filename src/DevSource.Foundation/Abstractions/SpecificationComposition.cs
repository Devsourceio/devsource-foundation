namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Represents composition modes for nested specifications.
/// </summary>
public enum SpecificationComposition
{
    /// <summary>
    /// No composition.
    /// </summary>
    None,

    /// <summary>
    /// Conjunction composition.
    /// </summary>
    And,

    /// <summary>
    /// Disjunction composition.
    /// </summary>
    Or,

    /// <summary>
    /// Negation composition.
    /// </summary>
    Not,
}