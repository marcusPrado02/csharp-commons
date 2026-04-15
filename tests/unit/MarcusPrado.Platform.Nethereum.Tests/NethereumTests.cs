using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Nethereum.Tests;

public sealed class NethereumOptionsTests
{
    [Fact]
    public void DefaultRpcUrl_PointsToLocalhost()
    {
        var opts = new NethereumOptions();
        opts.RpcUrl.Should().Be("http://localhost:8545");
    }

    [Fact]
    public void DefaultChainId_Is1337()
    {
        var opts = new NethereumOptions();
        opts.ChainId.Should().Be(1337);
    }

    [Fact]
    public void PrivateKeyDefaultsToNull()
    {
        var opts = new NethereumOptions();
        opts.PrivateKey.Should().BeNull();
    }
}

public sealed class ContractRegistryTests
{
    [Fact]
    public void Register_AddsAbi()
    {
        var registry = new ContractRegistry();
        registry.Register("0xABC", "[{\"type\":\"function\"}]");

        var abi = registry.GetAbi("0xABC");
        abi.Should().NotBeNull();
    }

    [Fact]
    public void GetAbi_CaseInsensitive()
    {
        var registry = new ContractRegistry();
        registry.Register("0xABC", "[]");

        registry.GetAbi("0xabc").Should().NotBeNull();
        registry.GetAbi("0XABC").Should().NotBeNull();
    }

    [Fact]
    public void GetAbi_UnknownAddress_ReturnsNull()
    {
        var registry = new ContractRegistry();
        registry.GetAbi("0xUNKNOWN").Should().BeNull();
    }
}

public sealed class NethereumWalletManagerTests
{
    [Fact]
    public async Task CreateWalletAsync_ReturnsNonEmptyAddress()
    {
        var manager = new NethereumWalletManager();
        var wallet = await manager.CreateWalletAsync();

        wallet.Address.Should().NotBeNullOrWhiteSpace();
        wallet.Address.Should().StartWith("0x");
        wallet.PublicKey.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateWalletAsync_EachCallReturnsDistinctAddress()
    {
        var manager = new NethereumWalletManager();
        var w1 = await manager.CreateWalletAsync();
        var w2 = await manager.CreateWalletAsync();

        w1.Address.Should().NotBe(w2.Address);
    }

    [Fact]
    public async Task SignMessageAsync_KnownWallet_ReturnsHexSignature()
    {
        var manager = new NethereumWalletManager();
        var wallet = await manager.CreateWalletAsync();

        var sig = await manager.SignMessageAsync(wallet.Address, [0x01, 0x02, 0x03]);

        sig.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SignMessageAsync_UnknownAddress_ThrowsInvalidOperationException()
    {
        var manager = new NethereumWalletManager();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.SignMessageAsync("0xUnknown", [0x01]));
    }
}

public sealed class NethereumSmartContractClientTests
{
    [Fact]
    public async Task CallAsync_NoAbiRegistered_ThrowsInvalidOperationException()
    {
        var web3 = Substitute.For<IWeb3>();
        var registry = new ContractRegistry();
        var client = new NethereumSmartContractClient(web3, registry);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.CallAsync<string>("0xNOCONTRACT", "balanceOf", ["0xADDR"]));
    }

    [Fact]
    public async Task SendAsync_NoAbiRegistered_ThrowsInvalidOperationException()
    {
        var web3 = Substitute.For<IWeb3>();
        var registry = new ContractRegistry();
        var client = new NethereumSmartContractClient(web3, registry);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.SendAsync("0xNOCONTRACT", "transfer", ["0xTO", 100]));
    }
}

public sealed class NethereumExtensionsTests
{
    [Fact]
    public void AddPlatformNethereum_RegistersAllBlockchainInterfaces()
    {
        var services = new ServiceCollection();
        services.AddPlatformNethereum(o => o.RpcUrl = "http://localhost:8545");

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IBlockchainClient>()
            .Should().BeOfType<NethereumBlockchainClient>();
        sp.GetRequiredService<IWalletManager>()
            .Should().BeOfType<NethereumWalletManager>();
        sp.GetRequiredService<ISmartContractClient>()
            .Should().BeOfType<NethereumSmartContractClient>();
    }
}
