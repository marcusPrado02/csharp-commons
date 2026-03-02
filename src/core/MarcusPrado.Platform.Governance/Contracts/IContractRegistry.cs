using MarcusPrado.Platform.Governance.Deprecation;

namespace MarcusPrado.Platform.Governance.Contracts;

/// <summary>Manages the platform's contract inventory.</summary>
public interface IContractRegistry
{
    /// <summary>Returns all registered contracts.</summary>
    IReadOnlyList<ContractMetadata> GetAll();

    /// <summary>Finds a contract by its name, or <c>null</c> if not found.</summary>
    ContractMetadata? GetByName(string name);

    /// <summary>Registers a new contract version.</summary>
    void Register(ContractRegistration registration);

    /// <summary>Marks a contract as deprecated with an optional notice.</summary>
    void Deprecate(string name, DeprecationNotice? notice = null);

    /// <summary>Marks a contract as retired (requests using it should be rejected).</summary>
    void Retire(string name);
}
