using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace MarcusPrado.Platform.AspNetCore.Localization;

/// <summary>
/// Extension methods for adding and using the platform localization infrastructure.
/// </summary>
public static class LocalizationExtensions
{
    /// <summary>
    /// Registers platform localization services into the DI container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">
    /// Optional delegate to configure <see cref="PlatformLocalizationOptions"/>.
    /// When omitted, defaults apply (<c>DefaultCulture = "en-US"</c>,
    /// <c>SupportedCultures = ["en-US", "pt-BR", "es-ES"]</c>).
    /// </param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddPlatformLocalization(opts =>
    /// {
    ///     opts.DefaultCulture = "pt-BR";
    ///     opts.SupportedCultures = ["en-US", "pt-BR"];
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddPlatformLocalization(
        this IServiceCollection services,
        Action<PlatformLocalizationOptions>? configure = null)
    {
        // Configure options.
        var optionsBuilder = services.AddOptions<PlatformLocalizationOptions>();
        if (configure is not null)
            optionsBuilder.Configure(configure);

        // Register ASP.NET Core's built-in localization (IStringLocalizerFactory, etc.).
        services.AddLocalization();

        // Register platform-specific localization services.
        // Note: LocalizedErrorTranslator is a static class — use it directly via LocalizedErrorTranslator.Translate(...)
        services.AddTransient<ValidationMessageLocalizer>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="AcceptLanguageMiddleware"/> to the ASP.NET Core middleware pipeline.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    /// <remarks>
    /// Call this before any middleware that needs to be culture-aware (e.g. routing,
    /// controller dispatch, minimal API endpoints).
    /// </remarks>
    public static IApplicationBuilder UsePlatformLocalization(this IApplicationBuilder app)
    {
        app.UseMiddleware<AcceptLanguageMiddleware>();
        return app;
    }
}
