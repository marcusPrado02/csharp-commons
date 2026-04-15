using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MarcusPrado.Platform.HealthChecks.Checks;

/// <summary>
/// An <see cref="IHealthCheck"/> that always returns <see cref="HealthStatus.Degraded"/>
/// with a configurable reason message.
/// </summary>
public sealed class DegradedHealthCheck : IHealthCheck
{
    private readonly string _reason;

    /// <summary>Initialises the check with the given reason message.</summary>
    /// <param name="reason">The message included in every degraded result.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="reason"/> is null or whitespace.</exception>
    public DegradedHealthCheck(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason must not be null or whitespace.", nameof(reason));

        _reason = reason;
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(HealthCheckResult.Degraded(_reason));
}
