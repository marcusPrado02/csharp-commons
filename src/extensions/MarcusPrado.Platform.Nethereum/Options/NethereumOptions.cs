namespace MarcusPrado.Platform.Nethereum.Options;

/// <summary>Configuration for the Nethereum blockchain adapter.</summary>
public sealed class NethereumOptions
{
    /// <summary>Gets or sets the Ethereum JSON-RPC endpoint URL.</summary>
    public string RpcUrl { get; set; } = "http://localhost:8545";

    /// <summary>Gets or sets the default account private key used for sending transactions.</summary>
    public string? PrivateKey { get; set; }

    /// <summary>Gets or sets the chain ID (e.g. 1 = mainnet, 11155111 = Sepolia).</summary>
    public long ChainId { get; set; } = 1337;
}
