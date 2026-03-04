namespace MarcusPrado.Platform.Abstractions.Blockchain;

/// <summary>Low-level blockchain client that reads and writes on-chain data.</summary>
public interface IBlockchainClient
{
    /// <summary>Gets the native-token balance for the given address.</summary>
    Task<string> GetBalanceAsync(string address, CancellationToken ct = default);

    /// <summary>Submits a transaction and returns the receipt.</summary>
    Task<TransactionReceipt> SendTransactionAsync(BlockchainTransaction tx, CancellationToken ct = default);

    /// <summary>Fetches the receipt for an already-submitted transaction.</summary>
    Task<TransactionReceipt> GetReceiptAsync(string txHash, CancellationToken ct = default);
}

/// <summary>Manages on-chain wallet operations.</summary>
public interface IWalletManager
{
    /// <summary>Creates a new wallet and returns its address and public key.</summary>
    Task<Wallet> CreateWalletAsync(CancellationToken ct = default);

    /// <summary>Signs a message using the private key of the given wallet address.</summary>
    Task<string> SignMessageAsync(string walletAddress, byte[] message, CancellationToken ct = default);
}

/// <summary>Interacts with a deployed smart contract.</summary>
public interface ISmartContractClient
{
    /// <summary>Calls a read-only contract function and returns the result.</summary>
    Task<T> CallAsync<T>(string contractAddress, string function, object?[] args, CancellationToken ct = default);

    /// <summary>Sends a state-changing transaction to a contract function.</summary>
    Task<string> SendAsync(string contractAddress, string function, object?[] args, CancellationToken ct = default);
}
