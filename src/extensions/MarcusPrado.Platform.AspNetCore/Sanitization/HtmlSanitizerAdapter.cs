using System.Text.RegularExpressions;
using Ganss.Xss;

namespace MarcusPrado.Platform.AspNetCore.Sanitization;

/// <summary>Wraps <see cref="Ganss.Xss.HtmlSanitizer"/> to implement <see cref="IInputSanitizer"/>.</summary>
public sealed partial class HtmlSanitizerAdapter : IInputSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    [GeneratedRegex("<[^>]*>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagPattern();

    /// <summary>Initializes a new instance of <see cref="HtmlSanitizerAdapter"/>.</summary>
    public HtmlSanitizerAdapter()
    {
        _sanitizer = new HtmlSanitizer();
        // Allow safe subset only — defaults are already quite restrictive
    }

    /// <inheritdoc/>
    public string SanitizeHtml(string input)
        => string.IsNullOrEmpty(input) ? input : _sanitizer.Sanitize(input);

    /// <inheritdoc/>
    public string StripHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        // Use regex to strip all HTML tags, preserving text content
        return HtmlTagPattern().Replace(input, string.Empty);
    }
}
