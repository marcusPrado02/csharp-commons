using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MarcusPrado.Platform.AspNetCore.RateLimiting;

/// <summary>
/// Sliding-window rate-limit policy partitioned by the authenticated user's
/// subject claim (falls back to IP address or <c>__anon__</c>).
/// </summary>
public sealed class UserRateLimitPolicy : IRateLimiterPolicy<string>
{
    private readonly PlatformRateLimitingOptions _options;

    /// <summary>Initialises with platform options.</summary>
    public UserRateLimitPolicy(PlatformRateLimitingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc/>
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? httpContext.Connection.RemoteIpAddress?.ToString()
                     ?? "__anon__";

        return RateLimitPartition.GetSlidingWindowLimiter(
            userId,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit       = _options.UserPermitLimit,
                Window            = _options.UserWindow,
                SegmentsPerWindow = _options.UserSegmentsPerWindow,
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
