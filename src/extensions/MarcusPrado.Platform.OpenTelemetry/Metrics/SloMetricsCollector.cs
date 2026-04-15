using System.Diagnostics.Metrics;
using MarcusPrado.Platform.Observability.SLO;

namespace MarcusPrado.Platform.OpenTelemetry.Metrics;

/// <summary>
/// Registers OpenTelemetry <see cref="ObservableGauge{T}"/> instruments for
/// <c>slo.availability</c> and <c>slo.error_budget_remaining</c> using
/// <see cref="System.Diagnostics.Metrics.Meter"/>.
/// </summary>
public sealed class SloMetricsCollector : IDisposable
{
    private readonly Meter _meter;

    /// <summary>
    /// Initializes a new instance of <see cref="SloMetricsCollector"/> and registers
    /// the <c>slo.availability</c> and <c>slo.error_budget_remaining</c> gauges.
    /// </summary>
    /// <param name="slo">The service level objective that defines the target and name.</param>
    /// <param name="snapshotProvider">
    /// A delegate called on each measurement cycle to obtain the current <see cref="SloSnapshot"/>.
    /// </param>
    public SloMetricsCollector(ServiceLevelObjective slo, Func<SloSnapshot> snapshotProvider)
    {
        ArgumentNullException.ThrowIfNull(slo);
        ArgumentNullException.ThrowIfNull(snapshotProvider);

        _meter = new Meter($"MarcusPrado.Platform.Slo.{slo.Name}", "1.0.0");

        var sloName = slo.Name;
        var target = slo.Target;

        _meter.CreateObservableGauge(
            name: "slo.availability",
            observeValue: () =>
            {
                var snapshot = snapshotProvider();
                var result = ErrorBudgetCalculator.Calculate(
                    snapshot,
                    new ServiceLevelObjective(sloName, target, slo.Window)
                );
                return result.AvailabilityRate;
            },
            unit: "ratio",
            description: $"Availability rate for SLO '{sloName}'."
        );

        _meter.CreateObservableGauge(
            name: "slo.error_budget_remaining",
            observeValue: () =>
            {
                var snapshot = snapshotProvider();
                var result = ErrorBudgetCalculator.Calculate(
                    snapshot,
                    new ServiceLevelObjective(sloName, target, slo.Window)
                );
                return result.ErrorBudgetRemaining;
            },
            unit: "ratio",
            description: $"Remaining error budget for SLO '{sloName}'."
        );
    }

    /// <inheritdoc/>
    public void Dispose() => _meter.Dispose();
}
