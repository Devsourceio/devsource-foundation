namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Represents a field in a provider-agnostic specification model.
/// </summary>
public sealed class SpecificationField
{
    /// <summary>
    /// Gets the field name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the field path.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the field CLR type.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationField"/> class.
    /// </summary>
    /// <param name="name">Field name.</param>
    /// <param name="path">Field path.</param>
    /// <param name="type">Field CLR type.</param>
    public SpecificationField(string name, string path, Type type)
    {
        Name = name;
        Path = path;
        Type = type;
    }    
}