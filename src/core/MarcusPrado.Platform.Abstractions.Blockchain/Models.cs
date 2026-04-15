namespace MarcusPrado.Platform.Abstractions.Blockchain;

/// <summary>Represents a request to submit a transaction on a blockchain network.</summary>
/// <param name="From">The sender wallet address.</param>
/// <param name="To">The recipient wallet address.</param>
/// <param name="Amount">The value to transfer, in the network's native token unit.</param>
/// <param name="Data">Optional encoded contract call data or message payload.</param>
/// <param name="GasPrice">Optional gas price override (in Gwei or equivalent).</param>
public sealed record BlockchainTransaction(
    string From,
    string To,
    decimal Amount,
    string? Data = null,
    decimal? GasPrice = null
);

/// <summary>Contains the on-chain outcome of a submitted blockchain transaction.</summary>
/// <param name="TxHash">The unique transaction hash assigned by the network.</param>
/// <param name="Success">Indicates whether the transaction was executed successfully.</param>
/// <param name="BlockNumber">The block number in which the transaction was included.</param>
/// <param name="GasUsed">The amount of gas consumed by the transaction.</param>
/// <param name="Error">Optional error message when <paramref name="Success"/> is <see langword="false"/>.</param>
public sealed record TransactionReceipt(
    string TxHash,
    bool Success,
    long BlockNumber,
    long GasUsed,
    string? Error = null
);

/// <summary>Represents a blockchain wallet with its public address and key.</summary>
/// <param name="Address">The public wallet address (e.g. Ethereum checksummed hex).</param>
/// <param name="PublicKey">The wallet's public key in hex-encoded form.</param>
public sealed record Wallet(string Address, string PublicKey);
