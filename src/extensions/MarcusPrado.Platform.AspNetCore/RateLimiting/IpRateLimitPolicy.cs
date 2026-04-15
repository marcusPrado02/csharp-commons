using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MarcusPrado.Platform.AspNetCore.RateLimiting;

/// <summary>
/// Fixed-window rate-limit policy partitioned by the client's remote IP
/// address (or <c>__unknown__</c> when the address is unavailable).
/// </summary>
public sealed class IpRateLimitPolicy : IRateLimiterPolicy<string>
{
    private readonly PlatformRateLimitingOptions _options;

    /// <summary>Initialises with platform options.</summary>
    public IpRateLimitPolicy(PlatformRateLimitingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc/>
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "__unknown__";

        return RateLimitPartition.GetFixedWindowLimiter(
            ip,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = _options.IpPermitLimit,
                Window = _options.IpWindow,
            });
    }

    /// <inheritdoc/>
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected
        => static (context, _) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
            }

            return ValueTask.CompletedTask;
        };
}
