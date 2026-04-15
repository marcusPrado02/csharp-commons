using Microsoft.Extensions.Hosting;

namespace MarcusPrado.Platform.Runtime.Lifecycle;

/// <summary>
/// Registers a callback to be invoked once the application has started.
/// The callback is fire-and-forget from the hosted service perspective;
/// exceptions are surfaced as <see cref="AggregateException"/> on the returned task.
/// </summary>
public static class StartupHook
{
    /// <summary>
    /// Wires <paramref name="handler"/> to <see cref="IHostApplicationLifetime.ApplicationStarted"/>.
    /// </summary>
    public static void Register(IHostApplicationLifetime lifetime, Func<CancellationToken, Task> handler)
    {
        lifetime.ApplicationStarted.Register(() => handler(lifetime.ApplicationStopping).GetAwaiter().GetResult());
    }
}
