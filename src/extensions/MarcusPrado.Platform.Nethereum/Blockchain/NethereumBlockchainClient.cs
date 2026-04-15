using System.Collections.Concurrent;
using System.Numerics;
using MarcusPrado.Platform.Abstractions.Blockchain;
using MarcusPrado.Platform.Nethereum.Options;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace MarcusPrado.Platform.Nethereum.Blockchain;

/// <summary>Implements <see cref="IBlockchainClient"/> via Nethereum.</summary>
public sealed class NethereumBlockchainClient : IBlockchainClient
{
    private readonly IWeb3 _web3;

    /// <summary>Initializes a new instance of <see cref="NethereumBlockchainClient"/>.</summary>
    public NethereumBlockchainClient(IWeb3 web3)
    {
        ArgumentNullException.ThrowIfNull(web3);
        _web3 = web3;
    }

    /// <inheritdoc />
    public async Task<string> GetBalanceAsync(string address, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);

        var wei = await _web3.Eth.GetBalance.SendRequestAsync(address).ConfigureAwait(false);
        var ether = Web3.Convert.FromWei(wei.Value);
        return ether.ToString("0.####################");
    }

    /// <inheritdoc />
    public async Task<Abstractions.Blockchain.TransactionReceipt> SendTransactionAsync(
        BlockchainTransaction tx, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(tx);

        var etherService = _web3.Eth.GetEtherTransferService();

#pragma warning disable CA2016
        var receipt = await etherService
            .TransferEtherAndWaitForReceiptAsync(
                tx.To,
                tx.Amount,
                tx.GasPrice.HasValue ? (decimal?)Web3.Convert.FromWei(
                    new BigInteger((long)(tx.GasPrice.Value * 1e9m))) : null)
            .ConfigureAwait(false);
#pragma warning restore CA2016

        return new Abstractions.Blockchain.TransactionReceipt(
            receipt.TransactionHash,
            receipt.Status?.Value == BigInteger.One,
            (long)receipt.BlockNumber.Value,
            (long)receipt.GasUsed.Value);
    }

    /// <inheritdoc />
    public async Task<Abstractions.Blockchain.TransactionReceipt> GetReceiptAsync(
        string txHash, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(txHash);

        var receipt = await _web3.Eth.Transactions.GetTransactionReceipt
            .SendRequestAsync(txHash).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Transaction receipt not found for hash '{txHash}'.");

        return new Abstractions.Blockchain.TransactionReceipt(
            receipt.TransactionHash,
            receipt.Status?.Value == BigInteger.One,
            (long)receipt.BlockNumber.Value,
            (long)receipt.GasUsed.Value);
    }
}

/// <summary>
/// Implements <see cref="IWalletManager"/> using an in-process key store.
/// Created wallets are kept in memory for the lifetime of the service.
/// </summary>
public sealed class NethereumWalletManager : IWalletManager
{
    private readonly ConcurrentDictionary<string, EthECKey> _keyStore = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<Wallet> CreateWalletAsync(CancellationToken ct = default)
    {
        var key = EthECKey.GenerateKey();
        var address = key.GetPublicAddress();
        var pubKey = Convert.ToHexString(key.GetPubKey());

        _keyStore[address] = key;

        return Task.FromResult(new Wallet(address, pubKey));
    }

    /// <inheritdoc />
    public Task<string> SignMessageAsync(
        string walletAddress, byte[] message, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(walletAddress);
        ArgumentNullException.ThrowIfNull(message);

        if (!_keyStore.TryGetValue(walletAddress, out var key))
        {
            throw new InvalidOperationException(
                $"No key found for wallet '{walletAddress}'. Use CreateWalletAsync first.");
        }

        var signer = new EthereumMessageSigner();
        var signature = signer.EncodeUTF8AndSign(
            System.Convert.ToBase64String(message), key);

        return Task.FromResult(signature);
    }
}
