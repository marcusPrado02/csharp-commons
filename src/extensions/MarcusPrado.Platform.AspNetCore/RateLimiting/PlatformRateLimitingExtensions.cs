using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MarcusPrado.Platform.AspNetCore.RateLimiting;

/// <summary>
/// Extension methods for registering the platform rate-limiting middleware.
/// </summary>
public static class PlatformRateLimitingExtensions
{
    /// <summary>Policy name for the per-tenant rate limit.</summary>
    public const string TenantPolicy = "platform-tenant";

    /// <summary>Policy name for the per-user rate limit.</summary>
    public const string UserPolicy = "platform-user";

    /// <summary>Policy name for the per-IP rate limit.</summary>
    public const string IpPolicy = "platform-ip";

    /// <summary>
    /// Adds the platform rate-limiting middleware with per-tenant, per-user,
    /// and per-IP policies.  The global <c>OnRejected</c> handler returns a
    /// 429 response with a <c>application/problem+json</c> body and an
    /// optional <c>Retry-After</c> header.
    /// Call <c>app.UseRateLimiter()</c> in the middleware pipeline to activate it.
    /// </summary>
    public static IServiceCollection AddPlatformRateLimiting(
        this IServiceCollection services,
        Action<PlatformRateLimitingOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new PlatformRateLimitingOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton<TenantRateLimitPolicy>();
        services.AddSingleton<UserRateLimitPolicy>();
        services.AddSingleton<IpRateLimitPolicy>();

        services.AddRateLimiter(limiter =>
        {
            limiter.AddPolicy<string, TenantRateLimitPolicy>(TenantPolicy);
            limiter.AddPolicy<string, UserRateLimitPolicy>(UserPolicy);
            limiter.AddPolicy<string, IpRateLimitPolicy>(IpPolicy);

            limiter.OnRejected = async (ctx, token) =>
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    ctx.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(
                        CultureInfo.InvariantCulture
                    );
                }

                const string Body =
                    "{\"status\":429,\"title\":\"Too Many Requests\","
                    + "\"detail\":\"Rate limit exceeded. Try again after the Retry-After period.\","
                    + "\"type\":\"https://tools.ietf.org/html/rfc6585#section-4\"}";

                ctx.HttpContext.Response.ContentType = "application/problem+json";
                await ctx.HttpContext.Response.WriteAsync(Body, token);
            };
        });

        return services;
    }
}
