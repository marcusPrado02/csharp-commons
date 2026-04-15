using System.Collections.ObjectModel;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.Redis.Stampede;

/// <summary>
/// <see cref="IHostedService"/> that runs registered warmup actions against
/// <see cref="IDistributedCache"/> at application startup.
/// Each warmup action is a <see cref="Func{IDistributedCache, CancellationToken, Task}"/>.
/// Exceptions thrown by individual actions are caught and logged so that remaining
/// actions still execute.
/// </summary>
public sealed class CacheWarmupService : IHostedService
{
    private static readonly Action<ILogger, int, Exception?> _logStarting =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(1, "WarmupStarting"),
            "Cache warmup starting. {Count} action(s) registered.");

    private static readonly Action<ILogger, Exception?> _logCompleted =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(2, "WarmupCompleted"),
            "Cache warmup completed.");

    private static readonly Action<ILogger, int, Exception?> _logActionFailed =
        LoggerMessage.Define<int>(
            LogLevel.Error,
            new EventId(3, "WarmupActionFailed"),
            "Cache warmup action {Index} threw an exception and was skipped.");

    private readonly IDistributedCache _cache;
    private readonly ReadOnlyCollection<Func<IDistributedCache, CancellationToken, Task>> _warmupActions;
    private readonly ILogger<CacheWarmupService> _logger;

    /// <summary>
    /// Initialises the warmup service with the distributed cache, warmup actions, and logger.
    /// </summary>
    /// <param name="cache">The distributed cache to pre-populate.</param>
    /// <param name="warmupActions">List of warmup actions to run on startup.</param>
    /// <param name="logger">Logger for reporting warmup progress and errors.</param>
    public CacheWarmupService(
        IDistributedCache cache,
        IEnumerable<Func<IDistributedCache, CancellationToken, Task>> warmupActions,
        ILogger<CacheWarmupService> logger)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(warmupActions);
        ArgumentNullException.ThrowIfNull(logger);

        _cache = cache;
        _warmupActions = warmupActions.ToList().AsReadOnly();
        _logger = logger;
    }

    /// <summary>
    /// Runs all registered warmup actions. Exceptions from individual actions are
    /// logged and do not prevent subsequent actions from running.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logStarting(_logger, _warmupActions.Count, null);

        for (var i = 0; i < _warmupActions.Count; i++)
        {
            try
            {
                await _warmupActions[i](_cache, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logActionFailed(_logger, i, ex);
            }
        }

        _logCompleted(_logger, null);
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
