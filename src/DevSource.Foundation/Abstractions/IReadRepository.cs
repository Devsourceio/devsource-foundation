namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Defines read-side query operations using provider-agnostic specifications.
/// </summary>
/// <typeparam name="TReadModel">The read model type.</typeparam>
public interface IReadRepository<TReadModel>
{
    /// <summary>
    /// Gets a single read model using a specification intended to identify it.
    /// </summary>
    /// <param name="specification">Query specification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching read model when found; otherwise <see langword="null"/>.</returns>
    Task<TReadModel?> GetByIdAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether at least one read model matches a specification.
    /// </summary>
    /// <param name="specification">Query specification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> when at least one model matches; otherwise <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a single read model by a specification.
    /// </summary>
    /// <param name="specification">Query specification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The read model when found; otherwise <see langword="null"/>.</returns>
    Task<TReadModel?> GetAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first read model that matches the specification.
    /// </summary>
    /// <param name="specification">Query specification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first matching model when found; otherwise <see langword="null"/>.</returns>
    Task<TReadModel?> FirstOrDefaultAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all read models that match the specification.
    /// </summary>
    /// <param name="specification">Query specification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of matching models.</returns>
    Task<IReadOnlyList<TReadModel>> ListAsync(ISpecification<TReadModel> specification, CancellationToken cancellationToken = default);
}
