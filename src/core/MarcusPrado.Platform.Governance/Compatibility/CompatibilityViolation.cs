namespace MarcusPrado.Platform.Governance.Compatibility;

/// <summary>Describes a single breaking change between two schema versions.</summary>
public sealed record CompatibilityViolation(ViolationType Type, string FieldPath, string Description);
