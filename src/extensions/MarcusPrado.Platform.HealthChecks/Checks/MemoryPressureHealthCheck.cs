using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MarcusPrado.Platform.HealthChecks.Checks;

/// <summary>Options for <see cref="MemoryPressureHealthCheck"/>.</summary>
public sealed class MemoryPressureOptions
{
    /// <summary>Allocated bytes above which the check reports Degraded (default: 512 MB).</summary>
    public long DegradedThresholdBytes { get; set; } = 512L * 1024 * 1024;

    /// <summary>Allocated bytes above which the check reports Unhealthy (default: 1 GB).</summary>
    public long UnhealthyThresholdBytes { get; set; } = 1024L * 1024 * 1024;
}

/// <summary>
/// Health check that monitors managed GC heap pressure.
/// Returns <see cref="HealthStatus.Degraded"/> or <see cref="HealthStatus.Unhealthy"/>
/// when allocated bytes exceed configured thresholds.
/// </summary>
public sealed class MemoryPressureHealthCheck : IHealthCheck
{
    private readonly MemoryPressureOptions _options;

    /// <summary>Initialises with memory pressure options.</summary>
    public MemoryPressureHealthCheck(IOptions<MemoryPressureOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);

        var data = new Dictionary<string, object>
        {
            ["allocated_bytes"] = allocated,
            ["degraded_threshold_bytes"] = _options.DegradedThresholdBytes,
            ["unhealthy_threshold_bytes"] = _options.UnhealthyThresholdBytes,
        };

        if (allocated >= _options.UnhealthyThresholdBytes)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Memory pressure critical: {allocated:N0} bytes allocated.",
                data: data));
        }

        if (allocated >= _options.DegradedThresholdBytes)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Memory pressure elevated: {allocated:N0} bytes allocated.",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Memory pressure normal: {allocated:N0} bytes allocated.",
            data: data));
    }
}
