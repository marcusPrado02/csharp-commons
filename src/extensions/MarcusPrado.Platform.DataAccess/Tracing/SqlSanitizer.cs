using System.Text.RegularExpressions;

namespace MarcusPrado.Platform.DataAccess.Tracing;

public static partial class SqlSanitizer
{
    // Replace single-quoted string literals
    [GeneratedRegex(@"'[^']*'")]
    private static partial Regex StringLiterals();

    // Replace numeric literals (standalone numbers)
    [GeneratedRegex(@"\b\d+\b")]
    private static partial Regex NumericLiterals();

    /// <summary>Replaces all literal values with '?' placeholders.</summary>
    public static string Sanitize(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return sql;
        var s = StringLiterals().Replace(sql, "?");
        return NumericLiterals().Replace(s, "?");
    }
}
