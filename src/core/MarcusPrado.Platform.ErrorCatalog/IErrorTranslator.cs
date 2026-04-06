using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.ErrorCatalog;

/// <summary>
/// Translates a structured <see cref="Error"/> into a human-readable message,
/// optionally for a specific locale.
/// </summary>
public interface IErrorTranslator
{
    /// <summary>
    /// Returns the human-readable message for <paramref name="error"/> in the
    /// requested <paramref name="culture"/> (e.g. <c>"pt-BR"</c>, <c>"en-US"</c>).
    /// </summary>
    /// <param name="error">The error to translate.</param>
    /// <param name="culture">
    /// The BCP-47 culture tag for the desired locale.
    /// When <see langword="null"/> or empty the implementation should use a
    /// sensible default (typically the current thread culture or <c>"en"</c>).
    /// </param>
    /// <returns>
    /// A localized human-readable message. Falls back to <see cref="Error.Message"/>
    /// when no translation is registered for the given code and culture.
    /// </returns>
    string Translate(Error error, string? culture = null);
}
