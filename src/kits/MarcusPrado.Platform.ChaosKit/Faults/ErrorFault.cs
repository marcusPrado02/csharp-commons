namespace MarcusPrado.Platform.ChaosKit.Faults;

/// <summary>
/// Injects an exception fault into an operation based on a configured injection rate.
/// </summary>
public sealed class ErrorFault
{
    private readonly ChaosConfig _config;

    /// <summary>
    /// Initialises a new <see cref="ErrorFault"/> with the specified <paramref name="config"/>.
    /// </summary>
    /// <param name="config">The chaos configuration that controls injection rate and fault exception.</param>
    public ErrorFault(ChaosConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
    }

    /// <summary>
    /// Conditionally throws <see cref="ChaosConfig.FaultException"/> based on
    /// <see cref="ChaosConfig.InjectionRate"/>.
    /// </summary>
    /// <exception cref="Exception">
    /// Throws <see cref="ChaosConfig.FaultException"/> when injection rate triggers and a
    /// fault exception is configured.
    /// </exception>
    public void Inject()
    {
        if (_config.FaultException is null)
            return;
        if (Random.Shared.NextDouble() >= _config.InjectionRate)
            return;

        throw _config.FaultException;
    }
}
