namespace MarcusPrado.Platform.Resilience.Policies;

/// <summary>Applies a hard timeout to any async operation.</summary>
public sealed class TimeoutPolicy
{
    private readonly TimeSpan _timeout;

    /// <summary>Initialises with the specified timeout.</summary>
    public TimeoutPolicy(TimeSpan timeout) => _timeout = timeout;

    /// <summary>Executes <paramref name="action"/> bounded by the timeout.</summary>
    /// <exception cref="TimeoutException">Thrown when the timeout elapses.</exception>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        using var cts =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        try
        {
            return await action(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"The operation timed out after {_timeout}.");
        }
    }
}
