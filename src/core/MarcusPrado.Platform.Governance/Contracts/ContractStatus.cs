namespace MarcusPrado.Platform.Governance.Contracts;

/// <summary>The lifecycle status of a registered contract.</summary>
public enum ContractStatus
{
    /// <summary>Contract is active and fully supported.</summary>
    Active,

    /// <summary>Contract is deprecated; clients should migrate to a newer version.</summary>
    Deprecated,

    /// <summary>Contract is retired; all requests using it will be rejected.</summary>
    Retired,
}
