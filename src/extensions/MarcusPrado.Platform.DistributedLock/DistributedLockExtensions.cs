namespace MarcusPrado.Platform.DistributedLock;

/// <summary>Fluent helpers for <see cref="IDistributedLock"/>.</summary>
public static class DistributedLockExtensions
{
    /// <summary>
    /// Acquires an exclusive lock on <paramref name="key"/>, executes
    /// <paramref name="action"/>, then releases the lock.
    /// </summary>
    /// <param name="lock">The distributed lock provider.</param>
    /// <param name="key">The resource identifier to lock.</param>
    /// <param name="expiry">Automatic expiry (time-to-live) for the lock.</param>
    /// <param name="action">The asynchronous action to execute while holding the lock.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the lock cannot be acquired (i.e. <see cref="IDistributedLock.AcquireAsync"/>
    /// returns <c>null</c>).
    /// </exception>
    public static async Task WithLockAsync(
        this IDistributedLock @lock,
        string key,
        TimeSpan expiry,
        Func<Task> action,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@lock);
        ArgumentNullException.ThrowIfNull(action);

        var handle = await @lock.AcquireAsync(key, expiry, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"Could not acquire distributed lock for key '{key}'.");

        await using (handle)
        {
            await action().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Acquires an exclusive lock on <paramref name="key"/>, executes
    /// <paramref name="func"/>, releases the lock, and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The return type of the function.</typeparam>
    /// <param name="lock">The distributed lock provider.</param>
    /// <param name="key">The resource identifier to lock.</param>
    /// <param name="expiry">Automatic expiry (time-to-live) for the lock.</param>
    /// <param name="func">The asynchronous function to execute while holding the lock.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The value returned by <paramref name="func"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the lock cannot be acquired (i.e. <see cref="IDistributedLock.AcquireAsync"/>
    /// returns <c>null</c>).
    /// </exception>
    public static async Task<TResult> WithLockAsync<TResult>(
        this IDistributedLock @lock,
        string key,
        TimeSpan expiry,
        Func<Task<TResult>> func,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@lock);
        ArgumentNullException.ThrowIfNull(func);

        var handle = await @lock.AcquireAsync(key, expiry, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                $"Could not acquire distributed lock for key '{key}'.");

        await using (handle)
        {
            return await func().ConfigureAwait(false);
        }
    }
}
