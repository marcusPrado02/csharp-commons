namespace MarcusPrado.Platform.Runtime.Lifecycle;

/// <summary>
/// Orchestrates an ordered set of drain/shutdown callbacks with a configurable
/// timeout (default 30 s). Intended to be called once when the host is stopping.
/// </summary>
public sealed class GracefulShutdown
{
    private readonly List<Func<CancellationToken, Task>> _handlers = [];
    private readonly TimeSpan _timeout;

    /// <summary>Creates a new instance with an optional drain timeout.</summary>
    /// <param name="timeout">Maximum wait time; defaults to 30 seconds.</param>
    public GracefulShutdown(TimeSpan? timeout = null)
        => _timeout = timeout ?? TimeSpan.FromSeconds(30);

    /// <summary>Registers an asynchronous drain handler.</summary>
    public void Register(Func<CancellationToken, Task> handler)
        => _handlers.Add(handler);

    /// <summary>
    /// Runs all registered handlers concurrently, bounded by <see cref="_timeout"/>.
    /// </summary>
    public async Task RunAsync()
    {
        using var cts = new CancellationTokenSource(_timeout);
        var tasks = _handlers.Select(h => h(cts.Token));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
