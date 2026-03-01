namespace MarcusPrado.Platform.Abstractions.Errors;

/// <summary>
/// Translates an <see cref="Error"/> code or instance into a human-readable,
/// optionally localised message.
/// </summary>
public interface IErrorTranslator
{
    /// <summary>
    /// Returns the translated message for <paramref name="error"/> in the
    /// requested <paramref name="culture"/> (e.g. <c>"en-US"</c>, <c>"pt-BR"</c>).
    /// Falls back to the error's own <see cref="Error.Message"/> when no translation is found.
    /// </summary>
    string Translate(Error error, string culture = "en-US");

    /// <summary>
    /// Returns the translated message for the error identified by <paramref name="code"/>
    /// in the requested <paramref name="culture"/>.
    /// Returns an empty string when the code is unknown.
    /// </summary>
    string Translate(string code, string culture = "en-US");
}
