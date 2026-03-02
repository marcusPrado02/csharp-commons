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

    /// <summary>
    /// Adds the platform rate-limiting middleware with per-tenant and
    /// per-user policies.  Call <c>app.UseRateLimiter()</c> in the
    /// middleware pipeline to activate it.
    /// </summary>
    public static IServiceCollection AddPlatformRateLimiting(
        this IServiceCollection services,
        Action<PlatformRateLimitingOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new PlatformRateLimitingOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton<TenantRateLimitPolicy>();
        services.AddSingleton<UserRateLimitPolicy>();

        services.AddRateLimiter(limiter =>
        {
            limiter.AddPolicy<string, TenantRateLimitPolicy>(TenantPolicy);
            limiter.AddPolicy<string, UserRateLimitPolicy>(UserPolicy);
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}
