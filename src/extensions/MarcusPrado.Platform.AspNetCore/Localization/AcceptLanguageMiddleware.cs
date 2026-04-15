using System.Globalization;
using Microsoft.Extensions.Options;

namespace MarcusPrado.Platform.AspNetCore.Localization;

/// <summary>
/// ASP.NET Core middleware that reads the <c>Accept-Language</c> HTTP header, resolves the
/// best matching supported culture and sets <see cref="CultureInfo.CurrentCulture"/> /
/// <see cref="CultureInfo.CurrentUICulture"/> for the duration of the request.
/// </summary>
/// <remarks>
/// <para>
/// If the header is absent, empty, or contains only unsupported / malformed locales the
/// <see cref="PlatformLocalizationOptions.DefaultCulture"/> is used, so the middleware
/// never throws due to a bad header value.
/// </para>
/// <para>
/// When an <see cref="ILocalizationContext"/> is registered in the DI container the resolved
/// culture is stored on it so downstream components can read it without touching
/// <c>Thread.CurrentCulture</c>.
/// </para>
/// </remarks>
public sealed class AcceptLanguageMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PlatformLocalizationOptions _options;

    /// <summary>
    /// Initialises the middleware.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="options">Platform localization options (injected via DI).</param>
    public AcceptLanguageMiddleware(RequestDelegate next, IOptions<PlatformLocalizationOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    /// <summary>
    /// Processes the HTTP request, setting culture information from the
    /// <c>Accept-Language</c> header before invoking the next middleware.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var culture = ResolveRequestCulture(context);

        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        // Store on ILocalizationContext if registered.
        var localizationContext = context.RequestServices.GetService<ILocalizationContext>();
        localizationContext?.SetCulture(culture);

        try
        {
            await _next(context);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private CultureInfo ResolveRequestCulture(HttpContext context)
    {
        var headerValues = context.Request.Headers.AcceptLanguage;

        if (headerValues.Count == 0)
            return GetDefaultCulture();

        // Collect all locale candidates from all header values, sorted by quality descending.
        var candidates = headerValues
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .SelectMany(v => v!.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(ParseLanguageQuality)
            .Where(lq => lq.HasValue)
            .Select(lq => lq!.Value)
            .OrderByDescending(lq => lq.Quality);

        var matched = candidates.Select(lq => MatchSupportedCulture(lq.Locale)).FirstOrDefault(c => c is not null);

        return matched ?? GetDefaultCulture();
    }

    private CultureInfo? MatchSupportedCulture(string locale)
    {
        // Exact match first (e.g. "pt-BR").
        var exactMatch = _options.SupportedCultures.FirstOrDefault(s =>
            string.Equals(s, locale, StringComparison.OrdinalIgnoreCase)
        );
        if (exactMatch is not null)
            return TryCreateCulture(exactMatch);

        // Language-only fallback (e.g. "pt" matches "pt-BR").
        var languageOnly = locale.Split('-')[0];
        var languageMatch = _options.SupportedCultures.FirstOrDefault(s =>
            string.Equals(s.Split('-')[0], languageOnly, StringComparison.OrdinalIgnoreCase)
        );

        return languageMatch is not null ? TryCreateCulture(languageMatch) : null;
    }

    private static (string Locale, double Quality)? ParseLanguageQuality(string segment)
    {
        var parts = segment.Trim().Split(';');
        var locale = parts[0].Trim();

        if (string.IsNullOrEmpty(locale))
            return null;

        double quality = 1.0;
        if (parts.Length > 1)
        {
            var qPart = parts[1].Trim();
            if (
                qPart.StartsWith("q=", StringComparison.OrdinalIgnoreCase)
                && double.TryParse(qPart[2..], NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedQ)
            )
            {
                quality = parsedQ;
            }
        }

        return (locale, quality);
    }

    private static CultureInfo? TryCreateCulture(string name)
    {
        try
        {
            return CultureInfo.GetCultureInfo(name);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }

    private CultureInfo GetDefaultCulture() =>
        TryCreateCulture(_options.DefaultCulture) ?? CultureInfo.InvariantCulture;
}
