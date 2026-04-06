namespace MarcusPrado.Platform.ChaosKit.Faults;

/// <summary>
/// Simulates packet loss by conditionally skipping an action based on a configured injection rate.
/// </summary>
public sealed class PacketLossFault
{
    private readonly ChaosConfig _config;

    /// <summary>
    /// Initialises a new <see cref="PacketLossFault"/> with the specified <paramref name="config"/>.
    /// </summary>
    /// <param name="config">The chaos configuration that controls injection rate.</param>
    public PacketLossFault(ChaosConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
    }

    /// <summary>
    /// Executes <paramref name="action"/> unless a packet-loss fault is triggered, in which
    /// case the action is skipped entirely. The <paramref name="onResult"/> callback is always
    /// invoked with a boolean indicating whether the packet was dropped (<see langword="true"/>
    /// means dropped/lost).
    /// </summary>
    /// <param name="action">The send/publish action to potentially skip.</param>
    /// <param name="onResult">
    /// Callback invoked with <see langword="true"/> when the packet was dropped,
    /// or <see langword="false"/> when the action was executed normally.
    /// </param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InjectAsync(
        Func<Task> action,
        Action<bool> onResult,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(onResult);

        var dropped = Random.Shared.NextDouble() < _config.InjectionRate;

        if (dropped)
        {
            onResult(true);
            return;
        }

        await action().ConfigureAwait(false);
        onResult(false);
    }
}
