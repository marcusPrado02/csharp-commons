using MarcusPrado.Platform.AspNetCore.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MarcusPrado.Platform.AspNetCore.Extensions;

/// <summary>
/// Extension methods that register and activate the platform security-headers
/// middleware and HSTS for non-Development environments.
/// </summary>
public static class SecurityHeadersExtensions
{
    /// <summary>
    /// Registers <see cref="Security.SecurityHeadersOptions"/> as a singleton.
    /// Call this in <c>ConfigureServices</c> before
    /// <see cref="UseSecurityHeaders"/> or <see cref="WebApplicationExtensions.UsePlatformMiddlewares"/>.
    /// </summary>
    public static IServiceCollection AddPlatformSecurityHeaders(
        this IServiceCollection services,
        Action<Security.SecurityHeadersOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new Security.SecurityHeadersOptions();
        configure?.Invoke(opts);
        services.AddSingleton(opts);

        return services;
    }

    /// <summary>
    /// Adds <see cref="SecurityHeadersMiddleware"/> to the pipeline.
    /// Requires <see cref="AddPlatformSecurityHeaders"/> to have been called first.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }

    /// <summary>
    /// Activates ASP.NET Core's built-in HSTS middleware when the current
    /// environment is not Development and <see cref="Security.SecurityHeadersOptions.EnableHsts"/>
    /// is <c>true</c>.
    /// </summary>
    public static IApplicationBuilder UsePlatformHsts(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(env);

        var opts = app.ApplicationServices.GetRequiredService<Security.SecurityHeadersOptions>();

        if (opts.EnableHsts && !env.IsDevelopment())
            app.UseHsts();

        return app;
    }
}
