namespace MarcusPrado.Platform.Redis.Lock;

/// <summary>
/// Provides distributed mutual-exclusion locks backed by a shared store.
/// </summary>
public interface IDistributedLock
{
    /// <summary>
    /// Acquires an exclusive lock on <paramref name="key"/>, retrying up to
    /// <paramref name="retryCount"/> times before throwing.
    /// </summary>
    /// <param name="key">The resource identifier to lock.</param>
    /// <param name="ttl">Automatic expiry for the lock.</param>
    /// <param name="retryCount">Extra attempts after the first try (default: 3).</param>
    /// <param name="retryDelay">Delay between attempts (default: 100 ms).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A handle that releases the lock when disposed.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the lock cannot be acquired after all attempts.
    /// </exception>
    Task<ILockHandle> AcquireAsync(
        string key,
        TimeSpan ttl,
        int retryCount = 3,
        TimeSpan? retryDelay = null,
        CancellationToken ct = default);

    /// <summary>
    /// Tries to acquire the lock once.  Returns <c>null</c> when the lock
    /// is currently held by another holder.
    /// </summary>
    Task<ILockHandle?> TryAcquireAsync(
        string key,
        TimeSpan ttl,
        CancellationToken ct = default);
}
