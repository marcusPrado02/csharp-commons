using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.ExceptionEnrichment;

/// <summary>
/// Extension methods for registering and configuring the exception enrichment services.
/// </summary>
public static class ExceptionEnrichmentExtensions
{
    /// <summary>
    /// Registers the <see cref="DeveloperExceptionPageEnricher"/> middleware with the DI container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPlatformExceptionEnrichment(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<DeveloperExceptionPageEnricher>();
        return services;
    }

    /// <summary>
    /// Adds the <see cref="DeveloperExceptionPageEnricher"/> to the application's middleware pipeline.
    /// In non-Development environments the middleware is a no-op pass-through.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static IApplicationBuilder UsePlatformDeveloperExceptionPage(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<DeveloperExceptionPageEnricher>();
        return app;
    }
}
