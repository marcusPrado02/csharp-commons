namespace MarcusPrado.Platform.Abstractions.Blockchain;

public sealed record BlockchainTransaction(
    string From, string To, decimal Amount, string? Data = null, decimal? GasPrice = null);

public sealed record TransactionReceipt(
    string TxHash, bool Success, long BlockNumber, long GasUsed, string? Error = null);

public sealed record Wallet(
    string Address, string PublicKey);
