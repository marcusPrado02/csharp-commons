namespace MarcusPrado.Platform.ChaosKit;

/// <summary>
/// Configuration options for chaos fault injection.
/// </summary>
public sealed class ChaosConfig
{
    /// <summary>
    /// Gets or sets the probability (0.0 to 1.0) that a fault will be injected on each invocation.
    /// A value of <c>0.0</c> means never inject; <c>1.0</c> means always inject.
    /// </summary>
    public double InjectionRate { get; set; }

    /// <summary>
    /// Gets or sets the artificial latency delay to inject when
    /// <see cref="InjectionRate"/> triggers. <see langword="null"/> means no latency.
    /// </summary>
    public TimeSpan? LatencyDelay { get; set; }

    /// <summary>
    /// Gets or sets the exception to throw when <see cref="InjectionRate"/> triggers for
    /// error faults. <see langword="null"/> means no error fault is configured.
    /// </summary>
    public Exception? FaultException { get; set; }
}
