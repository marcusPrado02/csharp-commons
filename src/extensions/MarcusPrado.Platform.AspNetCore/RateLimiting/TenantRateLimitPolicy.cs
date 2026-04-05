using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MarcusPrado.Platform.AspNetCore.RateLimiting;

/// <summary>
/// Fixed-window rate-limit policy partitioned by the tenant identifier
/// extracted from the <c>X-Tenant-ID</c> request header.
/// </summary>
public sealed class TenantRateLimitPolicy : IRateLimiterPolicy<string>
{
    private readonly PlatformRateLimitingOptions _options;

    /// <summary>Initialises with platform options.</summary>
    public TenantRateLimitPolicy(PlatformRateLimitingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc/>
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var tenantId = httpContext.Request.Headers["X-Tenant-ID"].ToString();
        var key = string.IsNullOrEmpty(tenantId) ? "__anon__" : tenantId;

        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = _options.TenantPermitLimit,
                Window      = _options.TenantWindow,
            });
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns <see langword="null"/> so that the global <c>OnRejected</c>
    /// handler registered in <see cref="PlatformRateLimitingExtensions"/> is
    /// used to write the 429 ProblemDetails response.
    /// </remarks>
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => null;
}
