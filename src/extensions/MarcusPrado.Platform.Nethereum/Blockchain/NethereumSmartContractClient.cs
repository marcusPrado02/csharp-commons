using MarcusPrado.Platform.Abstractions.Blockchain;
using Nethereum.Web3;

namespace MarcusPrado.Platform.Nethereum.Blockchain;

/// <summary>Implements <see cref="ISmartContractClient"/> via Nethereum.</summary>
public sealed class NethereumSmartContractClient : ISmartContractClient
{
    private readonly IWeb3 _web3;
    private readonly ContractRegistry _registry;
    private readonly string _defaultFromAddress;

    /// <summary>Initializes a new instance of <see cref="NethereumSmartContractClient"/>.</summary>
    public NethereumSmartContractClient(IWeb3 web3, ContractRegistry registry, string defaultFromAddress = "")
    {
        ArgumentNullException.ThrowIfNull(web3);
        ArgumentNullException.ThrowIfNull(registry);
        _web3 = web3;
        _registry = registry;
        _defaultFromAddress = defaultFromAddress;
    }

    /// <inheritdoc />
    public async Task<T> CallAsync<T>(
        string contractAddress,
        string function,
        object?[] args,
        CancellationToken ct = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contractAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(function);
        ArgumentNullException.ThrowIfNull(args);

        var abi = GetRequiredAbi(contractAddress);
        var contract = _web3.Eth.GetContract(abi, contractAddress);
        var fn = contract.GetFunction(function);

        return await fn.CallAsync<T>(args).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string> SendAsync(
        string contractAddress,
        string function,
        object?[] args,
        CancellationToken ct = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contractAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(function);
        ArgumentNullException.ThrowIfNull(args);

        var abi = GetRequiredAbi(contractAddress);
        var contract = _web3.Eth.GetContract(abi, contractAddress);
        var fn = contract.GetFunction(function);

        return await fn.SendTransactionAsync(_defaultFromAddress, args).ConfigureAwait(false);
    }

    private string GetRequiredAbi(string contractAddress)
    {
        var abi =
            _registry.GetAbi(contractAddress)
            ?? throw new InvalidOperationException(
                $"No ABI registered for contract '{contractAddress}'. "
                    + "Register the ABI via ContractRegistry.Register() before calling smart contract methods."
            );
        return abi;
    }
}
