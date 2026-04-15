using Microsoft.Extensions.Hosting;

namespace MarcusPrado.Platform.Runtime.Lifecycle;

/// <summary>
/// Registers a callback to be invoked when the application is stopping.
/// </summary>
public static class ShutdownHook
{
    /// <summary>
    /// Wires <paramref name="handler"/> to <see cref="IHostApplicationLifetime.ApplicationStopping"/>.
    /// </summary>
    public static void Register(IHostApplicationLifetime lifetime, Func<CancellationToken, Task> handler)
    {
        lifetime.ApplicationStopping.Register(() => handler(lifetime.ApplicationStopping).GetAwaiter().GetResult());
    }
}
