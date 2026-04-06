using MarcusPrado.Platform.ChaosKit.Faults;

namespace MarcusPrado.Platform.ChaosKit.Harness;

/// <summary>
/// Applies configured chaos faults before executing an action.
/// </summary>
public static class ChaosRunner
{
    /// <summary>
    /// Applies all configured chaos faults in sequence (latency, then error) and — if no fault
    /// aborts execution — runs <paramref name="action"/>.
    /// </summary>
    /// <param name="config">The <see cref="ChaosConfig"/> controlling which faults to apply.</param>
    /// <param name="action">The action to run after fault injection.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A task that completes when the action (and any injected faults) have finished.</returns>
    /// <remarks>
    /// Fault application order:
    /// <list type="number">
    ///   <item><description>Latency (delay)</description></item>
    ///   <item><description>Error (exception throw)</description></item>
    ///   <item><description>Original <paramref name="action"/></description></item>
    /// </list>
    /// </remarks>
    public static async Task RunWithChaos(
        ChaosConfig config,
        Func<Task> action,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(action);

        // 1. Latency
        var latency = new LatencyFault(config);
        await latency.InjectAsync(ct).ConfigureAwait(false);

        // 2. Error — may throw
        var error = new ErrorFault(config);
        error.Inject();

        // 3. Action
        await action().ConfigureAwait(false);
    }
}
