namespace MarcusPrado.Platform.AspNetCore.IpFiltering;

/// <summary>
/// Extension methods that register and activate the IP filter middleware.
/// </summary>
public static class IpFilterExtensions
{
    /// <summary>
    /// Registers <see cref="IpFilterOptions"/> and <see cref="InMemoryIpFilterStore"/>
    /// as <see cref="IIpFilterStore"/>. Call this in <c>ConfigureServices</c>.
    /// </summary>
    public static IServiceCollection AddPlatformIpFilter(
        this IServiceCollection services,
        Action<IpFilterOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new IpFilterOptions();
        configure?.Invoke(opts);

        services.AddSingleton(opts);
        services.AddSingleton<IIpFilterStore, InMemoryIpFilterStore>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IpFilterMiddleware"/> to the pipeline.
    /// Requires <see cref="AddPlatformIpFilter"/> to have been called first.
    /// </summary>
    public static IApplicationBuilder UseIpFilter(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<IpFilterMiddleware>();
    }
}
