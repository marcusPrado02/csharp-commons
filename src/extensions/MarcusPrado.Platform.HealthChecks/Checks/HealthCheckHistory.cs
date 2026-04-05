using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MarcusPrado.Platform.HealthChecks.Checks;

/// <summary>
/// A single health check result with its timestamp and description.
/// </summary>
/// <param name="Name">The name of the health check that produced this record.</param>
/// <param name="Status">The <see cref="HealthStatus"/> reported by the check.</param>
/// <param name="CheckedAt">The UTC timestamp when the check was executed.</param>
/// <param name="Description">An optional human-readable description of the result.</param>
public sealed record HealthCheckRecord(
    string Name,
    HealthStatus Status,
    DateTimeOffset CheckedAt,
    string? Description);

/// <summary>
/// Stores the last <em>N</em> <see cref="HealthCheckRecord"/> results per check name,
/// implemented as a circular buffer backed by <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
public sealed class HealthCheckHistory
{
    private readonly ConcurrentDictionary<string, Queue<HealthCheckRecord>> _store = new();
    private readonly int _maxHistoryPerCheck;

    /// <summary>
    /// Initialises the history store.
    /// </summary>
    /// <param name="maxHistoryPerCheck">Maximum records retained per check name. Must be ≥ 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxHistoryPerCheck"/> is less than 1.</exception>
    public HealthCheckHistory(int maxHistoryPerCheck = 10)
    {
        if (maxHistoryPerCheck < 1)
            throw new ArgumentOutOfRangeException(nameof(maxHistoryPerCheck), "Must be at least 1.");

        _maxHistoryPerCheck = maxHistoryPerCheck;
    }

    /// <summary>Records a new entry for the given check name.</summary>
    /// <param name="record">The record to store.</param>
    public void Record(HealthCheckRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var queue = _store.GetOrAdd(record.Name, _ => new Queue<HealthCheckRecord>());

        lock (queue)
        {
            queue.Enqueue(record);
            while (queue.Count > _maxHistoryPerCheck)
                queue.Dequeue();
        }
    }

    /// <summary>Returns a snapshot of all recorded history, keyed by check name.</summary>
    /// <returns>A dictionary mapping each check name to its list of records (oldest first).</returns>
    public IReadOnlyDictionary<string, IReadOnlyList<HealthCheckRecord>> GetAll()
    {
        var result = new Dictionary<string, IReadOnlyList<HealthCheckRecord>>();

        foreach (var (name, queue) in _store)
        {
            lock (queue)
            {
                result[name] = queue.ToList();
            }
        }

        return result;
    }

    /// <summary>Returns the recorded history for a specific check name.</summary>
    /// <param name="name">The check name to look up.</param>
    /// <returns>The list of records (oldest first), or an empty list if none exist.</returns>
    public IReadOnlyList<HealthCheckRecord> GetByName(string name)
    {
        if (_store.TryGetValue(name, out var queue))
        {
            lock (queue)
            {
                return queue.ToList();
            }
        }

        return [];
    }
}
