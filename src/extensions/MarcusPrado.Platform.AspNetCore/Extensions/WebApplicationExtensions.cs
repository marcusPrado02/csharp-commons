using MarcusPrado.Platform.AspNetCore.Middleware;

namespace MarcusPrado.Platform.AspNetCore.Extensions;

/// <summary>
/// <see cref="IApplicationBuilder"/> / <see cref="WebApplication"/> extension
/// methods that wire up the platform middleware in the correct order.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Adds the platform middleware stack to the pipeline in the recommended order:
    /// <list type="number">
    ///   <item><see cref="RequestLoggingMiddleware"/> — earliest possible to capture all requests.</item>
    ///   <item><see cref="CorrelationMiddleware"/> — must run before logging so IDs are available.</item>
    ///   <item><see cref="ExceptionMiddleware"/> — catches all downstream exceptions.</item>
    ///   <item><see cref="TenantResolutionMiddleware"/> — runs after auth middleware (if any) so JWT claims are present.</item>
    /// </list>
    /// </summary>
    /// <param name="app">The application pipeline builder.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static IApplicationBuilder UsePlatformMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<CorrelationMiddleware>();
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<TenantResolutionMiddleware>();
        return app;
    }
}
