using System.Collections.Concurrent;
using MarcusPrado.Platform.Governance.Deprecation;

namespace MarcusPrado.Platform.Governance.Contracts;

/// <summary>
/// Thread-safe, in-process implementation of <see cref="IContractRegistry"/>.
/// Suitable for tests and single-instance scenarios.
/// </summary>
public sealed class InMemoryContractRegistry : IContractRegistry
{
    private readonly ConcurrentDictionary<string, ContractMetadata> _contracts = new(
        StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public IReadOnlyList<ContractMetadata> GetAll() =>
        _contracts.Values.ToList().AsReadOnly();

    /// <inheritdoc/>
    public ContractMetadata? GetByName(string name) =>
        _contracts.GetValueOrDefault(name);

    /// <inheritdoc/>
    public void Register(ContractRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        var metadata = new ContractMetadata
        {
            Name = registration.Name,
            Version = registration.Version,
            SchemaHash = registration.SchemaHash,
            SchemaJson = registration.SchemaJson,
        };

        _contracts[registration.Name] = metadata;
    }

    /// <inheritdoc/>
    public void Deprecate(string name, DeprecationNotice? notice = null)
    {
        if (_contracts.TryGetValue(name, out var existing))
        {
            _contracts[name] = existing with { Status = ContractStatus.Deprecated };
        }
    }

    /// <inheritdoc/>
    public void Retire(string name)
    {
        if (_contracts.TryGetValue(name, out var existing))
        {
            _contracts[name] = existing with { Status = ContractStatus.Retired };
        }
    }
}
