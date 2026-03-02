namespace MarcusPrado.Platform.Governance.Tests.ADR;

public sealed class InMemoryAdrStoreTests
{
    private static AdrRecord CreateRecord(int number = 1) =>
        new(number, "Use CQRS", AdrStatus.Accepted, new DateOnly(2025, 1, 15),
            ["Alice", "Bob"], "We need a command bus.", "Adopt CQRS without MediatR.", "Fast, testable.");

    [Fact]
    public async Task SaveAsync_StoresRecord()
    {
        var store = new InMemoryAdrStore();
        var record = CreateRecord(1);

        await store.SaveAsync(record);

        var result = await store.GetByNumberAsync(1);
        result.Should().Be(record);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRecordsOrderedByNumber()
    {
        var store = new InMemoryAdrStore();
        await store.SaveAsync(CreateRecord(3));
        await store.SaveAsync(CreateRecord(1));
        await store.SaveAsync(CreateRecord(2));

        var all = await store.GetAllAsync();

        all.Select(r => r.Number).Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task GetByNumberAsync_ReturnsNull_WhenNotFound()
    {
        var store = new InMemoryAdrStore();

        var result = await store.GetByNumberAsync(99);

        result.Should().BeNull();
    }
}
