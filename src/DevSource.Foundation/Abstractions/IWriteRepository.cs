namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Defines write-side persistence operations for aggregate or entity roots.
/// </summary>
/// <typeparam name="TWriteModel">The write-side model type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public interface IWriteRepository<TWriteModel, in TId>
{
    /// <summary>
    /// Adds a new write model for persistence.
    /// </summary>
    /// <param name="model">Write model instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(TWriteModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a write model from persistence.
    /// </summary>
    /// <param name="model">Write model instance.</param>
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
