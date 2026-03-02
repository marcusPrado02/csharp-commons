namespace MarcusPrado.Platform.Governance.Contracts;

/// <summary>Input used to register a new contract.</summary>
public sealed record ContractRegistration(
    string Name,
    string Version,
    string SchemaHash,
    string? SchemaJson = null);
