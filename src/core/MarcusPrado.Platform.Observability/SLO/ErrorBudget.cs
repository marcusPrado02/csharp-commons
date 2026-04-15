namespace MarcusPrado.Platform.Observability.SLO;

/// <summary>
/// The result of an error budget calculation for a given <see cref="ServiceLevelObjective"/>.
/// </summary>
/// <param name="AvailabilityRate">The measured availability rate as a fraction between 0 and 1.</param>
/// <param name="ErrorBudgetConsumed">The fraction of the error budget that has been consumed (0 to 1+).</param>
/// <param name="ErrorBudgetRemaining">The fraction of the error budget still available (may be negative when exhausted).</param>
/// <param name="IsBudgetExhausted">Indicates whether the error budget has been fully consumed.</param>
public sealed record ErrorBudgetResult(
    double AvailabilityRate,
    double ErrorBudgetConsumed,
    double ErrorBudgetRemaining,
    bool IsBudgetExhausted
);

/// <summary>
/// Calculates error budget metrics for a <see cref="ServiceLevelObjective"/> given a <see cref="SloSnapshot"/>.
/// </summary>
public static class ErrorBudgetCalculator
{
    /// <summary>
    /// Calculates the availability rate, error budget consumed, and error budget remaining
    /// for the given snapshot and SLO target.
    /// </summary>
    /// <param name="snapshot">The point-in-time request count sample.</param>
    /// <param name="slo">The service level objective defining the target availability.</param>
    /// <returns>An <see cref="ErrorBudgetResult"/> with the computed metrics.</returns>
    public static ErrorBudgetResult Calculate(SloSnapshot snapshot, ServiceLevelObjective slo)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(slo);

        if (snapshot.TotalRequests <= 0)
        {
            return new ErrorBudgetResult(
                AvailabilityRate: 1.0,
                ErrorBudgetConsumed: 0.0,
                ErrorBudgetRemaining: 1.0 - slo.Target,
                IsBudgetExhausted: false
            );
        }

        var successfulRequests = snapshot.TotalRequests - snapshot.FailedRequests;
        var availabilityRate = (double)successfulRequests / snapshot.TotalRequests;

        var totalErrorBudget = 1.0 - slo.Target;
        var actualErrorRate = 1.0 - availabilityRate;

        double errorBudgetConsumed;
        double errorBudgetRemaining;

        if (totalErrorBudget <= 0.0)
        {
            errorBudgetConsumed = actualErrorRate > 0.0 ? 1.0 : 0.0;
            errorBudgetRemaining = 0.0;
        }
        else
        {
            errorBudgetConsumed = actualErrorRate / totalErrorBudget;
            errorBudgetRemaining = totalErrorBudget - actualErrorRate;
        }

        return new ErrorBudgetResult(
            AvailabilityRate: availabilityRate,
            ErrorBudgetConsumed: errorBudgetConsumed,
            ErrorBudgetRemaining: errorBudgetRemaining,
            IsBudgetExhausted: availabilityRate < slo.Target
        );
    }
}
