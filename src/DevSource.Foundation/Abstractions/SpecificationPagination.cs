namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Represents pagination settings in the neutral specification model.
/// </summary>
public sealed class SpecificationPagination
{
    /// <summary>
    /// Gets the number of items to skip.
    /// </summary>
    public int Skip { get; }

    /// <summary>
    /// Gets the number of items to take.
    /// </summary>
    public int Take { get; }

    /// <summary>
    /// Gets the optional cursor token.
    /// </summary>
    public string? Cursor { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificationPagination"/> class.
    /// </summary>
    /// <param name="skip">Number of items to skip.</param>
    /// <param name="take">Number of items to take.</param>
    /// <param name="cursor">Optional cursor token.</param>
    public SpecificationPagination(int skip, int take, string? cursor = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

        if (cursor is not null && string.IsNullOrWhiteSpace(cursor))
        {
            throw new ArgumentException("Cursor cannot be empty or whitespace.", nameof(cursor));
        }

        if (cursor is not null && skip != 0)
        {
            throw new ArgumentException("Cursor pagination cannot be combined with an offset.", nameof(skip));
        }

        Skip = skip;
        Take = take;
        Cursor = cursor;
    }    
}
