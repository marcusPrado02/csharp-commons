namespace MarcusPrado.Platform.Resilience.Policies;

/// <summary>
/// Bulkhead isolation — caps the maximum number of concurrent executions.
/// Requests that exceed the limit are immediately rejected.
/// </summary>
public sealed class BulkheadPolicy : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;

    /// <summary>Creates a bulkhead with the given max parallelism.</summary>
    public BulkheadPolicy(int maxParallelism) => _semaphore = new SemaphoreSlim(maxParallelism, maxParallelism);

    /// <summary>Executes <paramref name="action"/> within the bulkhead.</summary>
    /// <exception cref="BulkheadRejectedException">Thrown when the bulkhead is full.</exception>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default
    )
    {
        if (!await _semaphore.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            throw new BulkheadRejectedException("Bulkhead limit reached; request rejected.");
        }

        try
        {
            return await action(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;
        _semaphore.Dispose();
        _disposed = true;
    }
}

/// <summary>Thrown by <see cref="BulkheadPolicy"/> when the limit is exceeded.</summary>
public sealed class BulkheadRejectedException : Exception
{
    /// <inheritdoc />
    public BulkheadRejectedException(string message)
        : base(message) { }
}
