using System.Globalization;

namespace MarcusPrado.Platform.AspNetCore.Localization;

/// <summary>
/// Extends <see cref="MarcusPrado.Platform.Abstractions.Context.IRequestContext"/> with
/// culture information resolved from the current HTTP request.
/// </summary>
/// <remarks>
/// Implement this interface together with
/// <see cref="MarcusPrado.Platform.Abstractions.Context.IRequestContext"/> on your
/// scoped request-context class so that <see cref="AcceptLanguageMiddleware"/> can store
/// the resolved culture for downstream consumers.
/// </remarks>
public interface ILocalizationContext
{
    /// <summary>Gets the culture resolved for the current request.</summary>
    CultureInfo Culture { get; }

    /// <summary>
    /// Sets the culture for the current request.
    /// Called once by <see cref="AcceptLanguageMiddleware"/> at the start of each request.
    /// </summary>
    /// <param name="culture">The resolved <see cref="CultureInfo"/>.</param>
    void SetCulture(CultureInfo culture);
}
