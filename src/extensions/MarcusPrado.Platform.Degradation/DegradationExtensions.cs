using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Degradation;

/// <summary>
/// Extension methods for registering and activating the platform degradation subsystem.
/// </summary>
public static class DegradationExtensions
{
    /// <summary>
    /// Registers an <see cref="InMemoryDegradationController"/> as the singleton
    /// <see cref="IDegradationController"/> in the DI container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddPlatformDegradation(this IServiceCollection services)
    {
        services.AddSingleton<IDegradationController, InMemoryDegradationController>();
        return services;
    }

    /// <summary>
    /// Adds <see cref="DegradationMiddleware"/> to the ASP.NET Core request pipeline.
    /// Call this before <c>UseRouting</c> / <c>UseEndpoints</c> so that maintenance
    /// and read-only enforcement runs early in the pipeline.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to configure.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static IApplicationBuilder UsePlatformDegradation(this IApplicationBuilder app)
    {
        app.UseMiddleware<DegradationMiddleware>();
        return app;
    }
}
