namespace MarcusPrado.Platform.ChaosKit.Faults;

/// <summary>
/// Injects artificial latency into an operation based on a configured injection rate.
/// </summary>
public sealed class LatencyFault
{
    private readonly ChaosConfig _config;

    /// <summary>
    /// Initialises a new <see cref="LatencyFault"/> with the specified <paramref name="config"/>.
    /// </summary>
    /// <param name="config">The chaos configuration that controls injection rate and delay duration.</param>
    public LatencyFault(ChaosConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
    }

    /// <summary>
    /// Conditionally delays the current operation based on <see cref="ChaosConfig.InjectionRate"/>
    /// and <see cref="ChaosConfig.LatencyDelay"/>.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that completes after the injected delay (if triggered).</returns>
    public async Task InjectAsync(CancellationToken cancellationToken = default)
    {
        if (_config.LatencyDelay is null) return;
        if (Random.Shared.NextDouble() >= _config.InjectionRate) return;

        await Task.Delay(_config.LatencyDelay.Value, cancellationToken).ConfigureAwait(false);
    }
}
