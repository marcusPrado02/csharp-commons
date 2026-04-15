namespace MarcusPrado.Platform.EventSourcing.Projections;

/// <summary>
/// In-memory implementation of <see cref="IReadModelStore{T}"/> intended for testing and development.
/// </summary>
public sealed class InMemoryReadModelStore<T> : IReadModelStore<T>
    where T : class
{
    private readonly Dictionary<string, T> _store = new();

    /// <inheritdoc />
    public Task<T?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var value);
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task SaveAsync(string id, T readModel, CancellationToken cancellationToken = default)
    {
        _store[id] = readModel;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<T>>(_store.Values.ToList());
}
