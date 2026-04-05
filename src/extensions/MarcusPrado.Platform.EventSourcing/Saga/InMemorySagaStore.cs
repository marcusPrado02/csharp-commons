using System.Collections.Concurrent;

namespace MarcusPrado.Platform.EventSourcing.Saga;

/// <summary>
/// An in-memory implementation of <see cref="ISagaStore{TState}"/> backed by a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/>. Suitable for unit tests.
/// </summary>
/// <typeparam name="TState">The state type of the saga.</typeparam>
public sealed class InMemorySagaStore<TState> : ISagaStore<TState>
{
    private readonly ConcurrentDictionary<string, ISaga<TState>> _store = new();

    /// <inheritdoc/>
    public Task SaveAsync(ISaga<TState> saga, CancellationToken ct = default)
    {
        _store[saga.Id] = saga;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<ISaga<TState>?> LoadAsync(string sagaId, CancellationToken ct = default)
    {
        _store.TryGetValue(sagaId, out var saga);
        return Task.FromResult(saga);
    }

    /// <summary>
    /// Returns the number of sagas currently stored.
    /// </summary>
    public int Count => _store.Count;
}
