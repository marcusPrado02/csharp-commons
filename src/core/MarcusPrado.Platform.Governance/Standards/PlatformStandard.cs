namespace MarcusPrado.Platform.Governance.Standards;

/// <summary>Describes a platform engineering standard that services must adhere to.</summary>
public sealed record PlatformStandard(
    string Code,
    string Title,
    string Description,
    string Category,
    bool IsMandatory = true);
