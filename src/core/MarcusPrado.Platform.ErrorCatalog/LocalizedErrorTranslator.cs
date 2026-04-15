using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.ErrorCatalog;

/// <summary>
/// Dictionary-based implementation of <see cref="IErrorTranslator"/> that maps
/// <c>(code, culture)</c> pairs to translated messages and falls back to
/// <see cref="Error.Message"/> when no translation is registered.
/// </summary>
/// <remarks>
/// <para>
/// Register translations via <see cref="Register(string, string, string)"/> before
/// calling <see cref="Translate"/>. This class is thread-safe for reads after all
/// translations have been registered; concurrent writes require external locking.
/// </para>
/// <para>
/// For production use you may replace this with an implementation backed by
/// <c>IStringLocalizer</c> from <c>Microsoft.Extensions.Localization.Abstractions</c>.
/// </para>
/// </remarks>
public sealed class LocalizedErrorTranslator : IErrorTranslator
{
    // Key: (code, normalizedCulture)
    private readonly Dictionary<(string Code, string Culture), string> _translations = new(
        EqualityComparer<(string, string)>.Default
    );

    /// <summary>
    /// Registers a translation for the given error <paramref name="code"/> and
    /// <paramref name="culture"/>.
    /// </summary>
    /// <param name="code">The stable machine-readable error code (e.g. <c>"PAYMENT_001"</c>).</param>
    /// <param name="culture">The BCP-47 culture tag (e.g. <c>"pt-BR"</c>).</param>
    /// <param name="message">The localized human-readable message.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/>, <paramref name="culture"/>, or
    /// <paramref name="message"/> is null or whitespace.
    /// </exception>
    public void Register(string code, string culture, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));
        ArgumentException.ThrowIfNullOrWhiteSpace(culture, nameof(culture));
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

        _translations[(code, NormalizeCulture(culture))] = message;
    }

    /// <inheritdoc/>
    public string Translate(Error error, string? culture = null)
    {
        var normalizedCulture = NormalizeCulture(culture);

        if (_translations.TryGetValue((error.Code, normalizedCulture), out var translated))
        {
            return translated;
        }

        // Fall back to the error's built-in message.
        return error.Message;
    }

    private static string NormalizeCulture(string? culture) =>
        string.IsNullOrWhiteSpace(culture) ? "en" : culture.Trim().ToLowerInvariant();
}
