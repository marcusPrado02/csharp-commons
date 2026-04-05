namespace MarcusPrado.Platform.DistributedLock;

/// <summary>
/// Provides distributed mutual-exclusion locks backed by a shared store.
/// </summary>
public interface IDistributedLock
{
    /// <summary>
    /// Tries to acquire an exclusive lock on <paramref name="key"/>.
    /// Returns <c>null</c> when the lock is currently held by another holder.
    /// </summary>
    /// <param name="key">The resource identifier to lock.</param>
    /// <param name="expiry">Automatic expiry (time-to-live) for the lock.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// An <see cref="IAsyncDisposable"/> that releases the lock when disposed,
    /// or <c>null</c> if the lock could not be acquired.
    /// </returns>
    Task<IAsyncDisposable?> AcquireAsync(string key, TimeSpan expiry, CancellationToken ct = default);
}
