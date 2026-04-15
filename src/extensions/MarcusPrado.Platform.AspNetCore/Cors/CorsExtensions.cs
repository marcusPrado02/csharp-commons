using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace MarcusPrado.Platform.AspNetCore.Cors;

/// <summary>
/// Extension methods for registering platform CORS policies.
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Adds platform CORS services with the specified profile.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional action to configure <see cref="PlatformCorsOptions"/>.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPlatformCors(
        this IServiceCollection services,
        Action<PlatformCorsOptions>? configure = null
    )
    {
        var opts = new PlatformCorsOptions();
        configure?.Invoke(opts);
        services.AddSingleton(opts);

        services.AddCors(cors =>
        {
            cors.AddPolicy(
                CorsConstants.DefaultPolicy,
                builder =>
                {
                    switch (opts.Profile)
                    {
                        case PlatformCorsProfile.DevPermissive:
                            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                            break;

                        case PlatformCorsProfile.StagingRestricted:
                            builder
                                .WithOrigins(opts.AllowedOrigins)
                                .AllowAnyMethod()
                                .WithHeaders("Content-Type", "Authorization", "api-version", "x-correlation-id");
                            break;

                        case PlatformCorsProfile.ProductionLocked:
                            builder
                                .WithOrigins(opts.AllowedOrigins)
                                .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                                .WithHeaders("Content-Type", "Authorization", "api-version", "x-correlation-id")
                                .SetPreflightMaxAge(TimeSpan.FromHours(1));
                            break;
                    }
                }
            );

            if (opts.EnableTenantAwarePolicy)
            {
                cors.AddPolicy(CorsConstants.TenantPolicy, _ => { }); // placeholder, overridden by provider
            }
        });

        if (opts.EnableTenantAwarePolicy)
        {
            services.AddSingleton<ICorsPolicyProvider>(sp => new TenantAwareCorsPolicy(
                new DefaultCorsPolicyProvider(sp.GetRequiredService<IOptions<CorsOptions>>()),
                opts
            ));
        }

        return services;
    }
}
