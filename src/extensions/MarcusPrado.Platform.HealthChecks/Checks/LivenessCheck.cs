using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MarcusPrado.Platform.HealthChecks.Checks;

/// <summary>
/// Liveness probe — always reports <see cref="HealthStatus.Healthy"/> while
/// the process is running.
/// </summary>
public sealed class LivenessCheck : IHealthCheck
{
    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(HealthCheckResult.Healthy("Process is alive."));
}
