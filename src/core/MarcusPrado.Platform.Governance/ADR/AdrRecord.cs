namespace MarcusPrado.Platform.Governance.ADR;

/// <summary>
/// An Architecture Decision Record (ADR) capturing an architectural decision,
/// its rationale, and its consequences.
/// </summary>
public sealed record AdrRecord(
    int Number,
    string Title,
    AdrStatus Status,
    DateOnly Date,
    IReadOnlyList<string> DecisionMakers,
    string Context,
    string Decision,
    string Consequences);
