namespace MarcusPrado.Platform.AspNetCore.Sanitization;

/// <summary>Detects common SQL injection patterns in input strings.</summary>
public static class SqlInjectionDetector
{
    // Common SQL injection patterns
    private static readonly string[] _patterns =
    [
        "--", ";--", ";", "/*", "*/", "xp_",
        "UNION ", "UNION\t", "SELECT ", "SELECT\t",
        "DROP ", "INSERT ", "UPDATE ", "DELETE ",
        "EXEC ", "EXECUTE ", "CAST(", "CONVERT(",
        "CHAR(", "NCHAR("
    ];

    /// <summary>Returns true if the input contains common SQL injection patterns.</summary>
    public static bool ContainsSqlInjection(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        var upper = input.ToUpperInvariant();
        return _patterns.Any(p => upper.Contains(p.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase));
    }
}
