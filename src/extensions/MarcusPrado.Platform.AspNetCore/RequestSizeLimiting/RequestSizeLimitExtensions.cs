namespace MarcusPrado.Platform.AspNetCore.RequestSizeLimiting;

/// <summary>
/// Extension methods that register and activate the request size limit middleware.
/// </summary>
public static class RequestSizeLimitExtensions
{
    /// <summary>
    /// Registers <see cref="RequestSizeLimitOptions"/> as a singleton so that
    /// <see cref="RequestSizeLimitMiddleware"/> can be resolved from DI.
    /// Call this in <c>ConfigureServices</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional delegate to customise the options.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPlatformRequestSizeLimit(
        this IServiceCollection services,
        Action<RequestSizeLimitOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new RequestSizeLimitOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        return services;
    }

    /// <summary>
    /// Adds <see cref="RequestSizeLimitMiddleware"/> to the pipeline.
    /// <para>
    /// This middleware is <b>opt-in</b> and intentionally excluded from
    /// <c>UsePlatformMiddlewares</c> because body-size enforcement may have
    /// side-effects (e.g. disabling Kestrel's built-in body limit via
    /// <c>IHttpMaxRequestBodySizeFeature</c>) that not every service wants.
    /// </para>
    /// Requires <see cref="AddPlatformRequestSizeLimit"/> to have been called first.
    /// </summary>
    /// <param name="app">The application pipeline builder.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static IApplicationBuilder UseRequestSizeLimit(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<RequestSizeLimitMiddleware>();
    }
}
