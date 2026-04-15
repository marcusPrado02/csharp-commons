namespace MarcusPrado.Platform.Governance.Deprecation;

/// <summary>Human-readable notice attached to a deprecated contract.</summary>
public sealed record DeprecationNotice(
    string Message,
    string? MigrationGuideUrl = null,
    DateTimeOffset? EffectiveDate = null
);
