namespace MarcusPrado.Platform.EventSourcing.Projections;

public sealed class InMemoryReadModelStore<T> : IReadModelStore<T>
    where T : class
{
    private readonly Dictionary<string, T> _store = new();

    public Task<T?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var value);
        return Task.FromResult(value);
    }

    public Task SaveAsync(string id, T readModel, CancellationToken cancellationToken = default)
    {
        _store[id] = readModel;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<T>>(_store.Values.ToList());
}
