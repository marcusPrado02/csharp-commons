using MarcusPrado.Platform.Abstractions.Blockchain;
using MarcusPrado.Platform.Nethereum.Blockchain;
using MarcusPrado.Platform.Nethereum.Options;
using Microsoft.Extensions.DependencyInjection;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace MarcusPrado.Platform.Nethereum.Extensions;

/// <summary>Extension methods to register Nethereum blockchain services.</summary>
public static class NethereumExtensions
{
    /// <summary>
    /// Registers <see cref="IBlockchainClient"/>, <see cref="IWalletManager"/>,
    /// and <see cref="ISmartContractClient"/> backed by Nethereum.
    /// </summary>
    public static IServiceCollection AddPlatformNethereum(
        this IServiceCollection services,
        Action<NethereumOptions>? configure = null,
        Action<ContractRegistry>? registerContracts = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new NethereumOptions();
        configure?.Invoke(opts);

        var registry = new ContractRegistry();
        registerContracts?.Invoke(registry);

        services.AddSingleton(opts);
        services.AddSingleton(registry);

        services.AddSingleton<IWeb3>(_ =>
        {
            if (!string.IsNullOrEmpty(opts.PrivateKey))
            {
                var account = new Account(opts.PrivateKey, opts.ChainId);
                return new Web3(account, opts.RpcUrl);
            }

            return new Web3(opts.RpcUrl);
        });

        services.AddSingleton<IBlockchainClient, NethereumBlockchainClient>();
        services.AddSingleton<IWalletManager, NethereumWalletManager>();
        services.AddSingleton<ISmartContractClient>(sp => new NethereumSmartContractClient(
            sp.GetRequiredService<IWeb3>(),
            registry,
            opts.PrivateKey is not null ? new Account(opts.PrivateKey).Address : string.Empty
        ));

        return services;
    }
}
