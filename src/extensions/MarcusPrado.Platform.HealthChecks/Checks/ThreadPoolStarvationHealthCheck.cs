using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MarcusPrado.Platform.HealthChecks.Checks;

/// <summary>Options for <see cref="ThreadPoolStarvationHealthCheck"/>.</summary>
public sealed class ThreadPoolStarvationOptions
{
    /// <summary>Min available workers before Degraded (default: 10).</summary>
    public int DegradedMinAvailableWorkers { get; set; } = 10;

    /// <summary>Min available workers before Unhealthy (default: 2).</summary>
    public int UnhealthyMinAvailableWorkers { get; set; } = 2;
}

/// <summary>
/// Health check that detects thread-pool starvation by inspecting the number of
/// available worker threads via <see cref="ThreadPool.GetAvailableThreads"/>.
/// </summary>
public sealed class ThreadPoolStarvationHealthCheck : IHealthCheck
{
    private readonly ThreadPoolStarvationOptions _options;

    /// <summary>Initialises with starvation detection options.</summary>
    public ThreadPoolStarvationHealthCheck(IOptions<ThreadPoolStarvationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
        ThreadPool.GetMaxThreads(out var maxWorker, out _);

        var data = new Dictionary<string, object>
        {
            ["available_worker_threads"] = workerThreads,
            ["available_completion_port_threads"] = completionPortThreads,
            ["max_worker_threads"] = maxWorker,
        };

        if (workerThreads <= _options.UnhealthyMinAvailableWorkers)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    $"Thread pool starved: only {workerThreads} worker threads available.",
                    data: data
                )
            );
        }

        if (workerThreads <= _options.DegradedMinAvailableWorkers)
        {
            return Task.FromResult(
                HealthCheckResult.Degraded(
                    $"Thread pool pressure: {workerThreads} worker threads available.",
                    data: data
                )
            );
        }

        return Task.FromResult(
            HealthCheckResult.Healthy($"Thread pool healthy: {workerThreads} worker threads available.", data: data)
        );
    }
}
