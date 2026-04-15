namespace MarcusPrado.Platform.RateLimiting.Quotas;

/// <summary>Persistent counter store for quota / rate-limit enforcement.</summary>
public interface IQuotaStore
{
    /// <summary>
    /// Atomically increments the counter for <paramref name="key"/> and returns
    /// the new value.  The counter resets after <paramref name="windowSeconds"/>.
    /// </summary>
    Task<long> IncrementAsync(string key, long windowSeconds, CancellationToken ct = default);

    /// <summary>
    /// Increments the counter and returns <c>true</c> when the new value is
    /// within <paramref name="limit"/>; returns <c>false</c> and does NOT
    /// increment when the limit is already reached.
    /// </summary>
    Task<bool> TryConsumeAsync(string key, long limit, long windowSeconds, CancellationToken ct = default);

    /// <summary>Resets the counter for <paramref name="key"/>.</summary>
    Task ResetAsync(string key, CancellationToken ct = default);
}
