namespace MarcusPrado.Platform.Governance.Contracts;

/// <summary>
/// Immutable descriptor for a platform contract (API schema, event schema, etc.).
/// </summary>
public sealed record ContractMetadata
{
    /// <summary>Unique, stable contract name (e.g. <c>"order.created.v1"</c>).</summary>
    public required string Name { get; init; }

    /// <summary>Semantic version of the contract.</summary>
    public required string Version { get; init; }

    /// <summary>SHA-256 hash of the schema payload for change detection.</summary>
    public required string SchemaHash { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public ContractStatus Status { get; init; } = ContractStatus.Active;

    /// <summary>Optional JSON schema string for compatibility checks.</summary>
    public string? SchemaJson { get; init; }

    /// <summary>When the contract was registered.</summary>
    public DateTimeOffset RegisteredAt { get; init; } = DateTimeOffset.UtcNow;
}
