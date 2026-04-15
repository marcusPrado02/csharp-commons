using System.Text.Json;
using MarcusPrado.Platform.HealthChecks.Checks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MarcusPrado.Platform.HealthChecks.Extensions;

/// <summary>
/// Extension methods for registering platform health check endpoints.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers liveness, readiness, and detail health check endpoints.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>/health/live</c> — always healthy while the process is alive.</item>
    /// <item><c>/health/ready</c> — depends on the registered <see cref="IDependencyHealthProbe"/> list.</item>
    /// <item><c>/health/detail</c> — detailed JSON report (non-production only).</item>
    /// </list>
    /// </remarks>
    public static IApplicationBuilder UsePlatformHealthChecks(this IApplicationBuilder app, bool includeDetail = false)
    {
        app.UseHealthChecks("/health/live", new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") });

        app.UseHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") });

        if (includeDetail)
        {
            app.UseHealthChecks("/health/detail", new HealthCheckOptions { ResponseWriter = WriteDetailedJson });
        }

        return app;
    }

    /// <summary>
    /// Registers the platform liveness and readiness health checks.
    /// </summary>
    public static IServiceCollection AddPlatformHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<LivenessCheck>("liveness", tags: ["live"])
            .AddCheck<ReadinessCheck>("readiness", tags: ["ready"]);

        return services;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static async Task WriteDetailedJson(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            entries = report.Entries.ToDictionary(
                k => k.Key,
                v => new
                {
                    status = v.Value.Status.ToString(),
                    description = v.Value.Description,
                    exception = v.Value.Exception?.Message,
                }
            ),
        };
        await context.Response.WriteAsJsonAsync(result);
    }
}
