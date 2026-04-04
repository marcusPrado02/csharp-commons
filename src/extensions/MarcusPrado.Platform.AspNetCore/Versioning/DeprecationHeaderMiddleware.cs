using Asp.Versioning;

namespace MarcusPrado.Platform.AspNetCore.Versioning;

/// <summary>
/// Options that map API version strings (e.g. <c>"1.0"</c>) to their deprecation and sunset dates.
/// </summary>
public sealed class DeprecationOptions
{
    /// <summary>
    /// Maps version strings to <c>(DeprecationDate, SunsetDate)</c> tuples.
    /// </summary>
    public Dictionary<string, (DateTimeOffset Deprecation, DateTimeOffset? Sunset)> DeprecatedVersions { get; } = new();
}

/// <summary>
/// Middleware that adds <c>Deprecation</c> and (optionally) <c>Sunset</c> response headers
/// when the resolved API version is listed in <see cref="DeprecationOptions"/>.
/// </summary>
public sealed class DeprecationHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DeprecationOptions _options;

    /// <summary>Initializes a new instance of <see cref="DeprecationHeaderMiddleware"/>.</summary>
    public DeprecationHeaderMiddleware(RequestDelegate next, DeprecationOptions options)
    {
        _next = next;
        _options = options;
    }

    /// <summary>Invokes the middleware.</summary>
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var versionFeature = context.Features.Get<IApiVersioningFeature>();
            var version = versionFeature?.RequestedApiVersion?.ToString();
            if (version is not null && _options.DeprecatedVersions.TryGetValue(version, out var dates))
            {
                context.Response.Headers["Deprecation"] = dates.Deprecation.ToString("R");
                if (dates.Sunset.HasValue)
                    context.Response.Headers["Sunset"] = dates.Sunset.Value.ToString("R");
            }
            return Task.CompletedTask;
        });
        return _next(context);
    }
}

/// <summary>
/// Extension methods for registering and using <see cref="DeprecationHeaderMiddleware"/>.
/// </summary>
public static class DeprecationExtensions
{
    /// <summary>
    /// Registers <see cref="DeprecationOptions"/> in DI and optionally configures deprecated versions.
    /// </summary>
    public static IServiceCollection AddPlatformDeprecation(
        this IServiceCollection services,
        Action<DeprecationOptions>? configure = null)
    {
        var opts = new DeprecationOptions();
        configure?.Invoke(opts);
        services.AddSingleton(opts);
        return services;
    }

    /// <summary>
    /// Adds the <see cref="DeprecationHeaderMiddleware"/> to the request pipeline.
    /// Call after registering <see cref="AddPlatformDeprecation"/>.
    /// </summary>
    public static IApplicationBuilder UseDeprecationHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<DeprecationHeaderMiddleware>();
}
