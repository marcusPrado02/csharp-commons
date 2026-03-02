namespace MarcusPrado.Platform.Governance.ADR;

/// <summary>The RFC status of an Architecture Decision Record.</summary>
public enum AdrStatus
{
    /// <summary>Proposed status — under discussion.</summary>
    Proposed,

    /// <summary>Accepted and enforced.</summary>
    Accepted,

    /// <summary>Superseded by a newer ADR.</summary>
    Superseded,

    /// <summary>Deprecated and no longer relevant.</summary>
    Deprecated,
}
