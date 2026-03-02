namespace MarcusPrado.Platform.Governance.Standards;

/// <summary>Records a violation of a <see cref="PlatformStandard"/>.</summary>
public sealed record StandardViolation(
    PlatformStandard Standard,
    string Service,
    string Details,
    DateTimeOffset DetectedAt);
