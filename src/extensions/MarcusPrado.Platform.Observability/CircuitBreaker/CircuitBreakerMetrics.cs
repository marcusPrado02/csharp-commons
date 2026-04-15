using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Observability.CircuitBreaker;

/// <summary>
/// Registers OpenTelemetry instruments for circuit breaker observability.
/// </summary>
/// <remarks>
/// Instruments created:
/// <list type="bullet">
/// <item><c>circuit_breaker.state</c> — <see cref="ObservableGauge{T}"/> (0=Closed, 1=Open, 2=HalfOpen).</item>
/// <item><c>circuit_breaker.failures_total</c> — <see cref="Counter{T}"/> for failure counts.</item>
/// </list>
/// </remarks>
public sealed class CircuitBreakerMetrics : IDisposable
{
    /// <summary>The name of the <see cref="Meter"/> used by this class.</summary>
    public const string MeterName = "MarcusPrado.Platform.CircuitBreaker";

    private readonly Meter _meter;
    private readonly CircuitBreakerRegistry _registry;
    private readonly Counter<long> _failuresCounter;
    private bool _disposed;

    /// <summary>
    /// Initialises metrics instruments and binds them to the provided <paramref name="registry"/>.
    /// </summary>
    /// <param name="registry">The circuit breaker registry to observe.</param>
    public CircuitBreakerMetrics(CircuitBreakerRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
        _meter = new Meter(MeterName, "1.0.0");

        _meter.CreateObservableGauge(
            "circuit_breaker.state",
            observeValues: ObserveStates,
            unit: "state",
            description: "Current state of each circuit breaker (0=Closed, 1=Open, 2=HalfOpen)."
        );

        _failuresCounter = _meter.CreateCounter<long>(
            "circuit_breaker.failures_total",
            unit: "failures",
            description: "Total number of failures recorded per circuit breaker."
        );
    }

    /// <summary>Records a failure metric for the named circuit breaker.</summary>
    /// <param name="name">The circuit breaker name to tag the measurement with.</param>
    public void RecordFailure(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _failuresCounter.Add(1, new KeyValuePair<string, object?>("circuit_breaker.name", name));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _meter.Dispose();
            _disposed = true;
        }
    }

    // ── Private ──────────────────────────────────────────────────────────────

    private IEnumerable<Measurement<int>> ObserveStates()
    {
        foreach (var entry in _registry.GetAll())
        {
            yield return new Measurement<int>(
                (int)entry.State,
                new KeyValuePair<string, object?>("circuit_breaker.name", entry.Name)
            );
        }
    }
}
