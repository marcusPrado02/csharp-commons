using System.Globalization;
using System.Resources;
using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.AspNetCore.Localization;

/// <summary>
/// Translates <see cref="Error"/> objects into localized human-readable messages
/// using embedded <c>.resx</c> resource files bundled with this assembly.
/// </summary>
/// <remarks>
/// <para>
/// Resource lookup uses the error's <see cref="ErrorCategory"/> as the key
/// (e.g. <c>"NotFound"</c>, <c>"Unauthorized"</c>, <c>"Validation"</c>).
/// If no entry exists for the category the error's own <see cref="Error.Message"/> is
/// returned unchanged, preserving backward compatibility.
/// </para>
/// <para>
/// Falls back to <c>en-US</c> when the requested culture has no matching resource.
/// </para>
/// </remarks>
public static class LocalizedErrorTranslator
{
    // ResourceManager for the Errors resource family embedded in this assembly.
    private static readonly ResourceManager _resources = new(
        "MarcusPrado.Platform.AspNetCore.Resources.Errors",
        typeof(LocalizedErrorTranslator).Assembly
    );

    /// <summary>
    /// Returns a localized message for the given <paramref name="error"/> in the
    /// specified <paramref name="culture"/>.
    /// </summary>
    /// <param name="error">The error whose message should be translated.</param>
    /// <param name="culture">The target culture for the translation.</param>
    /// <returns>
    /// A localized message string; falls back to the error's original
    /// <see cref="Error.Message"/> if no resource entry is found.
    /// </returns>
    public static string Translate(Error error, CultureInfo culture)
    {
        var key = error.Category.ToString(); // e.g. "NotFound", "Validation"

        var localized = _resources.GetString(key, culture);
        return localized ?? error.Message;
    }

    /// <summary>
    /// Returns a localized message for the given <paramref name="error"/> using
    /// <see cref="CultureInfo.CurrentUICulture"/>.
    /// </summary>
    /// <param name="error">The error whose message should be translated.</param>
    /// <returns>A localized message string.</returns>
    public static string Translate(Error error) => Translate(error, CultureInfo.CurrentUICulture);
}
