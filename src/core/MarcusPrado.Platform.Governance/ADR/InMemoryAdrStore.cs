using System.Collections.Concurrent;

namespace MarcusPrado.Platform.Governance.ADR;

/// <summary>Thread-safe, in-process <see cref="IAdrStore"/> backed by a concurrent dictionary.</summary>
public sealed class InMemoryAdrStore : IAdrStore
{
    private readonly ConcurrentDictionary<int, AdrRecord> _records = new();

    /// <inheritdoc/>
    public Task<IReadOnlyList<AdrRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = (IReadOnlyList<AdrRecord>)_records.Values.OrderBy(r => r.Number).ToList().AsReadOnly();

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<AdrRecord?> GetByNumberAsync(int number, CancellationToken cancellationToken = default) =>
        Task.FromResult(_records.GetValueOrDefault(number));

    /// <inheritdoc/>
    public Task SaveAsync(AdrRecord record, CancellationToken cancellationToken = default)
    {
        _records[record.Number] = record;
        return Task.CompletedTask;
    }
}
