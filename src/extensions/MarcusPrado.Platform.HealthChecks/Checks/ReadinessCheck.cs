using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MarcusPrado.Platform.HealthChecks.Checks;

/// <summary>
/// Readiness probe — aggregates all registered <see cref="IDependencyHealthProbe"/>
/// implementations and reports <see cref="HealthStatus.Unhealthy"/> if any fails.
/// </summary>
public sealed class ReadinessCheck : IHealthCheck
{
    private readonly IEnumerable<IDependencyHealthProbe> _probes;

    /// <summary>Initialises with the registered dependency probes.</summary>
    public ReadinessCheck(IEnumerable<IDependencyHealthProbe> probes)
        => _probes = probes;

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var failures = new List<string>();

        foreach (var probe in _probes)
        {
            try
            {
                var (healthy, message) = await probe.CheckAsync(cancellationToken);
                if (!healthy)
                {
                    failures.Add($"{probe.Name}: {message}");
                }
            }
#pragma warning disable CA1031  // readiness must not bubble probe failures to the host
            catch (Exception ex)
            {
                failures.Add($"{probe.Name}: {ex.Message}");
            }
#pragma warning restore CA1031
        }

        return failures.Count == 0
            ? HealthCheckResult.Healthy("All dependencies ready.")
            : HealthCheckResult.Unhealthy($"Dependencies unavailable: {string.Join("; ", failures)}");
    }
}
