namespace MarcusPrado.Platform.Governance.Tests.Contracts;

public sealed class InMemoryContractRegistryTests
{
    private readonly InMemoryContractRegistry _registry = new();

    [Fact]
    public void Register_AddsContractToRegistry()
    {
        _registry.Register(new ContractRegistration("order.created.v1", "1.0.0", "abc123"));

        var contract = _registry.GetByName("order.created.v1");

        contract.Should().NotBeNull();
        contract!.Name.Should().Be("order.created.v1");
        contract.Version.Should().Be("1.0.0");
        contract.Status.Should().Be(ContractStatus.Active);
    }

    [Fact]
    public void GetAll_ReturnsAllRegisteredContracts()
    {
        _registry.Register(new ContractRegistration("a", "1.0", "hash1"));
        _registry.Register(new ContractRegistration("b", "1.0", "hash2"));

        _registry.GetAll().Should().HaveCount(2);
    }

    [Fact]
    public void GetByName_ReturnsNull_WhenNotFound()
    {
        var result = _registry.GetByName("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public void Deprecate_ChangesStatusToDeprecated()
    {
        _registry.Register(new ContractRegistration("order.created.v1", "1.0.0", "abc"));

        _registry.Deprecate("order.created.v1");

        _registry.GetByName("order.created.v1")!.Status.Should().Be(ContractStatus.Deprecated);
    }

    [Fact]
    public void Retire_ChangesStatusToRetired()
    {
        _registry.Register(new ContractRegistration("order.created.v1", "1.0.0", "abc"));

        _registry.Retire("order.created.v1");

        _registry.GetByName("order.created.v1")!.Status.Should().Be(ContractStatus.Retired);
    }

    [Fact]
    public void Register_IsThreadSafe_WhenCalledConcurrently()
    {
        Parallel.For(0, 50, i => _registry.Register(new ContractRegistration($"contract-{i}", "1.0", $"hash{i}")));

        _registry.GetAll().Should().HaveCount(50);
    }
}
