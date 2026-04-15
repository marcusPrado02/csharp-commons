namespace MarcusPrado.Platform.DlqReprocessing.Store;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IDlqStore"/>.
/// Suitable for testing and single-process deployments.
/// </summary>
public sealed class InMemoryDlqStore : IDlqStore
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DlqMessage>> _store = new(
        StringComparer.Ordinal
    );

    /// <inheritdoc />
    public Task<IReadOnlyList<DlqMessage>> GetAsync(string topic, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ct.ThrowIfCancellationRequested();

        if (_store.TryGetValue(topic, out var bucket))
        {
            IReadOnlyList<DlqMessage> result = [.. bucket.Values];
            return Task.FromResult(result);
        }

        return Task.FromResult<IReadOnlyList<DlqMessage>>([]);
    }

    /// <inheritdoc />
    public Task<DlqMessage?> GetByIdAsync(string topic, string id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(id);
        ct.ThrowIfCancellationRequested();

        DlqMessage? found = null;
        if (_store.TryGetValue(topic, out var bucket))
            bucket.TryGetValue(id, out found);

        return Task.FromResult(found);
    }

    /// <inheritdoc />
    public Task RequeueAsync(string topic, string id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(id);
        ct.ThrowIfCancellationRequested();

        RemoveFromBucket(topic, id);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(string topic, string id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(topic);
        ArgumentNullException.ThrowIfNull(id);
        ct.ThrowIfCancellationRequested();

        // In-memory: permanently removing a message is the same store operation as requeuing it.
        // Real adapters will differentiate (e.g. broker ACK vs discard).
        if (_store.TryGetValue(topic, out var bucket))
            bucket.TryRemove(id, out _);

        return Task.CompletedTask;
    }

    private void RemoveFromBucket(string topic, string id)
    {
        if (_store.TryGetValue(topic, out var bucket))
            bucket.TryRemove(id, out _);
    }

    /// <inheritdoc />
    public Task AddAsync(DlqMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ct.ThrowIfCancellationRequested();

        var bucket = _store.GetOrAdd(
            message.Topic,
            _ => new ConcurrentDictionary<string, DlqMessage>(StringComparer.Ordinal)
        );
        bucket[message.Id] = message;

        return Task.CompletedTask;
    }
}
