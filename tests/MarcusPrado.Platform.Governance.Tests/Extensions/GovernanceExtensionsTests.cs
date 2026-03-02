namespace MarcusPrado.Platform.Governance.Tests.Extensions;

public sealed class GovernanceExtensionsTests
{
    [Fact]
    public void AddPlatformGovernance_RegistersContractRegistry()
    {
        var services = new ServiceCollection();
        services.AddPlatformGovernance();
        var sp = services.BuildServiceProvider();

        var registry = sp.GetRequiredService<IContractRegistry>();
        registry.Should().BeOfType<InMemoryContractRegistry>();
    }

    [Fact]
    public void AddPlatformGovernance_RegistersAdrStore()
    {
        var services = new ServiceCollection();
        services.AddPlatformGovernance();
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IAdrStore>();
        store.Should().BeOfType<InMemoryAdrStore>();
    }

    [Fact]
    public void AddPlatformGovernance_ContractRegistryIsSingleton()
    {
        var services = new ServiceCollection();
        services.AddPlatformGovernance();
        var sp = services.BuildServiceProvider();

        var a = sp.GetRequiredService<IContractRegistry>();
        var b = sp.GetRequiredService<IContractRegistry>();

        a.Should().BeSameAs(b);
    }
}
