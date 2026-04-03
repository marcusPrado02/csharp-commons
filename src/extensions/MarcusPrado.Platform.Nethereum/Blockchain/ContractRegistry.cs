namespace MarcusPrado.Platform.Nethereum.Blockchain;

/// <summary>Maps Ethereum contract addresses to their ABI definitions.</summary>
public sealed class ContractRegistry
{
    private readonly Dictionary<string, string> _abis =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers a contract ABI for the given address.</summary>
    public ContractRegistry Register(string contractAddress, string abi)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contractAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(abi);

        _abis[contractAddress] = abi;
        return this;
    }

    /// <summary>Returns the ABI for the given contract address, or <see langword="null"/> if not registered.</summary>
    public string? GetAbi(string contractAddress) =>
        _abis.TryGetValue(contractAddress, out var abi) ? abi : null;
}
