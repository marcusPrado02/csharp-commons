using Microsoft.Extensions.Hosting;

namespace MarcusPrado.Platform.Redis.Caching;

/// <summary>
/// Base class for hosted services that pre-populate the cache at startup.
/// Subclasses override <see cref="WarmAsync"/> to load critical data.
/// </summary>
public abstract class CacheWarmupService : IHostedService
{
    /// <summary>The cache to pre-populate.</summary>
    protected ICache Cache { get; }

    /// <summary>Initialises with the injected cache.</summary>
    protected CacheWarmupService(ICache cache)
    {
        ArgumentNullException.ThrowIfNull(cache);
        Cache = cache;
    }

    /// <summary>Override to load data into <see cref="Cache"/> at application startup.</summary>
    protected abstract Task WarmAsync(CancellationToken ct);

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken) => WarmAsync(cancellationToken);

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
