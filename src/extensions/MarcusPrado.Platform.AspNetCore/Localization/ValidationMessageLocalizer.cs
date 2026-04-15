using System.Globalization;
using Microsoft.Extensions.Localization;

namespace MarcusPrado.Platform.AspNetCore.Localization;

/// <summary>
/// Wraps <see cref="IStringLocalizer"/> to provide localized validation messages with
/// automatic fallback to <c>en-US</c> when the requested culture has no translation.
/// </summary>
/// <remarks>
/// Register this as a scoped or transient service. It is designed to be consumed by
/// validation filters and FluentValidation validators that need culture-aware messages.
/// </remarks>
public sealed class ValidationMessageLocalizer
{
    private readonly IStringLocalizerFactory _factory;
    private static readonly CultureInfo _fallbackCulture = CultureInfo.GetCultureInfo("en-US");

    /// <summary>
    /// Initialises the localizer with the ASP.NET Core string-localizer factory.
    /// </summary>
    /// <param name="factory">The factory used to create <see cref="IStringLocalizer"/> instances.</param>
    public ValidationMessageLocalizer(IStringLocalizerFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Returns a localized string for the given <paramref name="key"/> in the current UI culture.
    /// Falls back to <c>en-US</c> when no entry is found.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <returns>The localized string, or the key itself when no resource is found.</returns>
    public string GetMessage(string key) => GetMessage(key, CultureInfo.CurrentUICulture);

    /// <summary>
    /// Returns a localized string for the given <paramref name="key"/> in the specified
    /// <paramref name="culture"/>. Falls back to <c>en-US</c> when no entry is found.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <param name="culture">The target culture.</param>
    /// <returns>The localized string, or the key itself when no resource is found.</returns>
    public string GetMessage(string key, CultureInfo culture)
    {
        var localizer = _factory.Create(
            baseName: "MarcusPrado.Platform.AspNetCore.Resources.Errors",
            location: typeof(ValidationMessageLocalizer).Assembly.GetName().Name!
        );

        using var scope = new CultureScope(culture);
        var result = localizer[key];

        if (result.ResourceNotFound && !culture.Equals(_fallbackCulture))
        {
            using var fallbackScope = new CultureScope(_fallbackCulture);
            var fallback = localizer[key];
            return fallback.ResourceNotFound ? key : fallback.Value;
        }

        return result.ResourceNotFound ? key : result.Value;
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Temporarily overrides <see cref="CultureInfo.CurrentUICulture"/> in a using block.
    /// </summary>
    private readonly struct CultureScope : IDisposable
    {
        private readonly CultureInfo _previous;

        internal CultureScope(CultureInfo culture)
        {
            _previous = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose() => CultureInfo.CurrentUICulture = _previous;
    }
}
