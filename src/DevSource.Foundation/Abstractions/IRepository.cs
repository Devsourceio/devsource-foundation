using DevSource.Stack.Abstractions;

namespace DevSource.Foundation.Abstractions;

/// <summary>
/// Composes write and read repository contracts without forcing equal read and write model types.
/// </summary>
/// <typeparam name="TWriteModel">The write-side model type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
/// <typeparam name="TReadModel">The read-side model type.</typeparam>
public interface IRepository<TWriteModel, in TId, TReadModel> :
    IWriteRepository<TWriteModel, TId>,
    IReadRepository<TReadModel>
{
}

/// <summary>
/// Composes write and read repository contracts when both sides share the same model type.
/// </summary>
/// <typeparam name="TModel">The shared model type used for write and read.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public interface IRepository<TModel, in TId> : IRepository<TModel, TId, TModel>
{
}

/// <summary>
/// Composes write and read repository contracts with a <see cref="Guid"/> identifier and shared model type.
/// </summary>
/// <typeparam name="TModel">The shared model type used for writing and read.</typeparam>
public interface IRepository<TModel> : IRepository<TModel, Guid, TModel>
{
}
