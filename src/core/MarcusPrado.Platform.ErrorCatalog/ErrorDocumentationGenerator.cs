using System.Reflection;
using System.Text;
using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.ErrorCatalog;

/// <summary>
/// Generates human-readable documentation for all errors declared in
/// <see cref="ErrorCatalog"/> using reflection.
/// </summary>
public static class ErrorDocumentationGenerator
{
    /// <summary>
    /// Generates a Markdown table that lists every <c>static readonly</c>
    /// <see cref="Error"/> field declared in <see cref="ErrorCatalog"/> and its
    /// nested classes.
    /// </summary>
    /// <returns>
    /// A string containing a Markdown table with three columns:
    /// <list type="bullet">
    ///   <item><description><b>Code</b> — the stable machine-readable error code.</description></item>
    ///   <item><description><b>Type</b> — the <see cref="ErrorCategory"/> of the error.</description></item>
    ///   <item><description><b>Message</b> — the default English message.</description></item>
    /// </list>
    /// </returns>
    public static string GenerateMarkdownTable()
    {
        var errors = CollectErrors(typeof(ErrorCatalog));

        var sb = new StringBuilder();
        sb.AppendLine("| Code | Type | Message |");
        sb.AppendLine("|------|------|---------|");

        foreach (var (code, category, message) in errors)
        {
            sb.AppendLine(
                $"| {EscapeMarkdown(code)} | {EscapeMarkdown(category.ToString())} | {EscapeMarkdown(message)} |"
            );
        }

        return sb.ToString();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static IEnumerable<(string Code, ErrorCategory Category, string Message)> CollectErrors(Type catalogType)
    {
        // Enumerate static readonly Error fields on the type itself.
        foreach (var field in GetErrorFields(catalogType))
        {
            var error = (Error)field.GetValue(null)!;
            yield return (error.Code, error.Category, error.Message);
        }

        // Recurse into nested classes.
        foreach (var nested in catalogType.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            foreach (var entry in CollectErrors(nested))
            {
                yield return entry;
            }
        }
    }

    private static IEnumerable<FieldInfo> GetErrorFields(Type type) =>
        type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(Error) && f.IsInitOnly);

    private static string EscapeMarkdown(string value) => value.Replace("|", "\\|", StringComparison.Ordinal);
}
