namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Defines write-side persistence operations for aggregate or entity roots.
/// </summary>
/// <typeparam name="TWriteModel">The write-side model type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public interface IWriteRepository<TWriteModel, in TId>
{
    /// <summary>
    /// Gets a write model by identifier.
    /// </summary>
    /// <param name="id">Write model identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The writing model when found; otherwise <see langword="null"/>.</returns>
    Task<TWriteModel?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a writing model exists for the provided identifier.
    /// </summary>
    /// <param name="id">Write model identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when the write model exists.</returns>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new writing model for persistence.
    /// </summary>
    /// <param name="model">Write a model instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(TWriteModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a write model from persistence.
    /// </summary>
    /// <param name="model">Write a model instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveAsync(TWriteModel model, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines write-side persistence operations with a <see cref="Guid"/> identifier.
/// </summary>
/// <typeparam name="TWriteModel">The write-side model type.</typeparam>
public interface IWriteRepository<TWriteModel> : IWriteRepository<TWriteModel, Guid>
{
}
