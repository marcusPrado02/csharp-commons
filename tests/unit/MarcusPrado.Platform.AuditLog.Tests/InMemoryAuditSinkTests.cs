namespace MarcusPrado.Platform.AuditLog.Tests;

public sealed class InMemoryAuditSinkTests
{
    private readonly InMemoryAuditSink _sink = new();

    [Fact]
    public async Task Log_SingleEntry_StoredInEntries()
    {
        var entry = AuditEntry.Create(AuditAction.Created, "Order", "ord-1");
        await _sink.LogAsync(entry);
        _sink.Entries.Should().HaveCount(1);
        _sink.Entries[0].ResourceId.Should().Be("ord-1");
    }

    [Fact]
    public async Task Log_MultipleEntries_PreservesOrder()
    {
        await _sink.LogAsync(AuditEntry.Create(AuditAction.Created, "Order", "o1"));
        await _sink.LogAsync(AuditEntry.Create(AuditAction.Updated, "Order", "o2"));
        _sink.Entries.Should().HaveCount(2);
        _sink.Entries[0].ResourceId.Should().Be("o1");
        _sink.Entries[1].ResourceId.Should().Be("o2");
    }

    [Fact]
    public async Task Query_ByResource_FiltersByResource()
    {
        await _sink.LogAsync(AuditEntry.Create(AuditAction.Created, "Order", "o1"));
        await _sink.LogAsync(AuditEntry.Create(AuditAction.Created, "Product", "p1"));
        var results = await _sink.QueryAsync("Order");
        results.Should().HaveCount(1);
        results[0].Resource.Should().Be("Order");
    }

    [Fact]
    public async Task Query_ByResourceAndId_FiltersBoth()
    {
        await _sink.LogAsync(AuditEntry.Create(AuditAction.Created, "Order", "o1"));
        await _sink.LogAsync(AuditEntry.Create(AuditAction.Updated, "Order", "o2"));
        var results = await _sink.QueryAsync("Order", "o1");
        results.Should().HaveCount(1);
        results[0].ResourceId.Should().Be("o1");
    }

    [Fact]
    public async Task Query_ByDateRange_FiltersCorrectly()
    {
        var past = DateTimeOffset.UtcNow.AddHours(-2);
        var future = DateTimeOffset.UtcNow.AddHours(1);
        await _sink.LogAsync(AuditEntry.Create(AuditAction.Created, "Order", "o1"));
        var results = await _sink.QueryAsync("Order", from: past, to: future);
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task Clear_RemovesAllEntries()
    {
        await _sink.LogAsync(AuditEntry.Create(AuditAction.Created, "Order", "o1"));
        _sink.Clear();
        _sink.Entries.Should().BeEmpty();
    }

    [Fact]
    public void AuditEntry_Create_SetsTimestampToUtcNow()
    {
        var before = DateTimeOffset.UtcNow;
        var entry = AuditEntry.Create(AuditAction.Login, "User", "u1");
        entry.Timestamp.Should().BeOnOrAfter(before);
        entry.Timestamp.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void AuditEntry_Create_SetsUniqueId()
    {
        var e1 = AuditEntry.Create(AuditAction.Created, "X", "1");
        var e2 = AuditEntry.Create(AuditAction.Created, "X", "1");
        e1.Id.Should().NotBe(e2.Id);
    }

    [Fact]
    public async Task Log_WithMetadata_StoresMetadata()
    {
        var meta = new Dictionary<string, string> { ["key"] = "value" };
        var entry = AuditEntry.Create(AuditAction.Custom, "Resource", "r1", metadata: meta);
        await _sink.LogAsync(entry);
        _sink.Entries[0].Metadata.Should().ContainKey("key");
    }

    [Fact]
    public async Task Log_WithActorAndTenant_StoresCorrectly()
    {
        var entry = AuditEntry.Create(AuditAction.Deleted, "Order", "o9", actorId: "actor-1", tenantId: "tenant-1");
        await _sink.LogAsync(entry);
        _sink.Entries[0].ActorId.Should().Be("actor-1");
        _sink.Entries[0].TenantId.Should().Be("tenant-1");
    }

    [Fact]
    public async Task AllAuditActions_CanBeLogged()
    {
        foreach (var action in Enum.GetValues<AuditAction>())
        {
            await _sink.LogAsync(AuditEntry.Create(action, "R", "1"));
        }
        _sink.Entries.Should().HaveCount(Enum.GetValues<AuditAction>().Length);
    }
}
