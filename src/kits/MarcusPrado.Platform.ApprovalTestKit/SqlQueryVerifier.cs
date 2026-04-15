using System.Text.RegularExpressions;

namespace MarcusPrado.Platform.ApprovalTestKit;

/// <summary>
/// Snapshots EF Core SQL query strings by normalising all whitespace sequences
/// (tabs, newlines, multiple spaces) to a single space and trimming both ends.
/// This makes assertions stable even when the query formatter adds or removes
/// line breaks between runs.
/// </summary>
public static class SqlQueryVerifier
{
    private static readonly Regex _whitespacePattern = new(@"\s+", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Normalises all internal whitespace in <paramref name="sql"/> to a single space
    /// and trims leading/trailing whitespace.
    /// </summary>
    /// <param name="sql">The SQL query string to normalise.</param>
    /// <returns>The normalised SQL string.</returns>
    public static string Normalise(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return _whitespacePattern.Replace(sql.Trim(), " ");
    }
}
