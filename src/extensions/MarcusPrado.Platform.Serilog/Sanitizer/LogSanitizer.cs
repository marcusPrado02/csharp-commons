namespace MarcusPrado.Platform.Serilog.Sanitizer;

/// <summary>Removes sensitive PII patterns from log message templates before logging.</summary>
public static class LogSanitizer
{
    private static readonly (string Pattern, string Replacement)[] Patterns =
    {
        // Email addresses
        (@"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}", "***@***.***"),

        // Credit card numbers (4 groups of 4 digits)
        (@"(?:\d[ \-]?){13,19}", "****"),

        // CPF (Brazilian tax ID)
        (@"\d{3}[.\- ]?\d{3}[.\- ]?\d{3}[.\- ]?\d{2}", "***.***.**/***-**"),
    };

    /// <summary>Redacts PII patterns from <paramref name="input"/> and returns the sanitised string.</summary>
    public static string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var result = input;
        foreach (var (pattern, replacement) in Patterns)
        {
            result = System.Text.RegularExpressions.Regex.Replace(result, pattern, replacement);
        }

        return result;
    }
}
