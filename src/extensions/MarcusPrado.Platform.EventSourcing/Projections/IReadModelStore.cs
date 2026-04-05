namespace MarcusPrado.Platform.EventSourcing.Projections;

public interface IReadModelStore<T>
    where T : class
{
    Task<T?> GetAsync(string id, CancellationToken cancellationToken = default);

    Task SaveAsync(string id, T readModel, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
}
