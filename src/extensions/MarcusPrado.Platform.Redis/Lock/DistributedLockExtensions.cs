namespace MarcusPrado.Platform.Redis.Lock;

/// <summary>Fluent helpers for <see cref="IDistributedLock"/>.</summary>
public static class DistributedLockExtensions
{
    /// <summary>
    /// Acquires <paramref name="key"/>, runs <paramref name="action"/>, then
    /// releases the lock.
    /// </summary>
    public static async Task WithLockAsync(
        this IDistributedLock @lock,
        string key,
        TimeSpan ttl,
        Func<Task> action,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@lock);
        ArgumentNullException.ThrowIfNull(action);

        await using var handle = await @lock.AcquireAsync(key, ttl, ct: ct);
        await action();
    }

    /// <summary>
    /// Acquires <paramref name="key"/>, runs <paramref name="func"/>, releases
    /// the lock, and returns the result.
    /// </summary>
    public static async Task<TResult> WithLockAsync<TResult>(
        this IDistributedLock @lock,
        string key,
        TimeSpan ttl,
        Func<Task<TResult>> func,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@lock);
        ArgumentNullException.ThrowIfNull(func);

        await using var handle = await @lock.AcquireAsync(key, ttl, ct: ct);
        return await func();
    }
}
