using Asp.Versioning;

namespace MarcusPrado.Platform.AspNetCore.Versioning;

/// <summary>
/// Extension methods to register API versioning in the DI container.
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Adds API versioning support reading version from URL segment, header (<c>api-version</c>),
    /// and media-type. Defaults to v1.0 when not specified.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional callback to further configure <see cref="ApiVersioningOptions"/>.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddPlatformApiVersioning(
        this IServiceCollection services,
        Action<ApiVersioningOptions>? configure = null)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true; // adds api-supported-versions / api-deprecated-versions headers
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("api-version"),
                new MediaTypeApiVersionReader("v"));
            configure?.Invoke(options);
        });

        return services;
    }
}
