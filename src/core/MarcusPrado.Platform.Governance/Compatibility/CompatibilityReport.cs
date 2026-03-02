namespace MarcusPrado.Platform.Governance.Compatibility;

/// <summary>
/// The result of comparing the current schema against a previous version.
/// </summary>
public sealed class CompatibilityReport
{
    /// <summary>
    /// <c>true</c> when no breaking changes were found between the two schemas.
    /// </summary>
    public bool IsCompatible => Violations.Count == 0;

    /// <summary>All detected breaking-change violations.</summary>
    public IReadOnlyList<CompatibilityViolation> Violations { get; }

    /// <summary>Initializes a report from the given violation list.</summary>
    public CompatibilityReport(IReadOnlyList<CompatibilityViolation> violations)
    {
        Violations = violations;
    }

    /// <summary>Returns a compatible (empty) report.</summary>
    public static CompatibilityReport Compatible() => new([]);
}
