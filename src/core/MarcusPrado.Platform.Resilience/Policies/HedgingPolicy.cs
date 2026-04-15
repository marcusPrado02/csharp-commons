namespace MarcusPrado.Platform.Resilience.Policies;

/// <summary>
/// Hedging policy — starts a duplicate request after <see cref="HedgingOptions.HedgingDelay"/>
/// if the first attempt has not yet completed, returning whichever finishes first.
/// </summary>
public sealed class HedgingPolicy
{
    private readonly HedgingOptions _options;

    /// <summary>Initialises with the provided options.</summary>
    public HedgingPolicy(HedgingOptions options) => _options = options;

    /// <summary>Executes <paramref name="action"/> with hedging.</summary>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default
    )
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var primaryTask = action(linkedCts.Token);

        // Wait for hedging delay OR primary completion, whichever comes first
        await Task.WhenAny(primaryTask, Task.Delay(_options.HedgingDelay, linkedCts.Token)).ConfigureAwait(false);

        if (primaryTask.IsCompletedSuccessfully)
        {
            return primaryTask.Result;
        }

        // Start hedged attempt
        var hedgedTask = action(linkedCts.Token);

        var winner = await Task.WhenAny(primaryTask, hedgedTask).ConfigureAwait(false);

        await linkedCts.CancelAsync().ConfigureAwait(false);

        return await winner.ConfigureAwait(false);
    }
}

/// <summary>Configuration for <see cref="HedgingPolicy"/>.</summary>
public sealed class HedgingOptions
{
    /// <summary>Delay before starting the hedged attempt (default 1 s).</summary>
    public TimeSpan HedgingDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>Maximum number of parallel hedged attempts (default 2).</summary>
    public int MaxHedgedAttempts { get; init; } = 2;
}
