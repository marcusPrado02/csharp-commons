namespace MarcusPrado.Platform.EfCore.Tests.DbContext;

public sealed class AppDbContextTests : IDisposable
{
    private readonly TestDbContext _ctx;

    public AppDbContextTests()
    {
        _ctx = TestDbContext.CreateInMemory(Guid.NewGuid().ToString());
        _ctx.Database.EnsureCreated();
    }

    // ── Audit – creation ──────────────────────────────────────────────────────

    [Fact]
    public async Task SaveChangesAsync_SetsAuditCreatedAt_WhenEntityIsAdded()
    {
        var entity = new AuditableTestEntity { Name = "Alpha" };
        _ctx.AuditableEntities.Add(entity);

        var before = DateTimeOffset.UtcNow;
        await _ctx.SaveChangesAsync();
        var after = DateTimeOffset.UtcNow;

        Assert.NotNull(entity.Audit);
        Assert.InRange(entity.Audit.CreatedAt, before, after);
    }

    [Fact]
    public async Task SaveChangesAsync_SetsAuditCreatedBy_WhenEntityIsAdded()
    {
        var entity = new AuditableTestEntity { Name = "Beta" };
        _ctx.AuditableEntities.Add(entity);
        await _ctx.SaveChangesAsync();

        Assert.Equal("system", entity.Audit.CreatedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_UpdatedAtIsNull_AfterInitialInsert()
    {
        var entity = new AuditableTestEntity { Name = "Gamma" };
        _ctx.AuditableEntities.Add(entity);
        await _ctx.SaveChangesAsync();

        Assert.Null(entity.Audit.UpdatedAt);
    }

    // ── Audit – update ────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveChangesAsync_SetsUpdatedAt_WhenEntityIsModified()
    {
        var entity = new AuditableTestEntity { Name = "Delta" };
        _ctx.AuditableEntities.Add(entity);
        await _ctx.SaveChangesAsync();

        entity.Name = "Delta-Updated";
        var before = DateTimeOffset.UtcNow;
        await _ctx.SaveChangesAsync();
        var after = DateTimeOffset.UtcNow;

        Assert.NotNull(entity.Audit.UpdatedAt);
        Assert.InRange(entity.Audit.UpdatedAt!.Value, before, after);
    }

    [Fact]
    public async Task SaveChangesAsync_PreservesCreatedAt_WhenEntityIsModified()
    {
        var entity = new AuditableTestEntity { Name = "Epsilon" };
        _ctx.AuditableEntities.Add(entity);
        await _ctx.SaveChangesAsync();
        var createdAt = entity.Audit.CreatedAt;

        entity.Name = "Epsilon-Updated";
        await _ctx.SaveChangesAsync();

        Assert.Equal(createdAt, entity.Audit.CreatedAt);
    }

    // ── Domain events ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveChangesAsync_DispatchesDomainEvents_WhenPublisherIsProvided()
    {
        var publisher = new FakeDomainEventPublisher();
        await using var ctx = TestDbContext.CreateInMemory(Guid.NewGuid().ToString(), publisher);
        await ctx.Database.EnsureCreatedAsync();

        var entity = new DomainEventTestEntity { Name = "EventSource" };
        entity.RaiseOrderCreated();
        ctx.DomainEventEntities.Add(entity);
        await ctx.SaveChangesAsync();

        Assert.Single(publisher.Published);
        Assert.Equal("order.created", publisher.Published[0].EventType);
    }

    [Fact]
    public async Task SaveChangesAsync_ClearsDomainEvents_AfterDispatch()
    {
        var publisher = new FakeDomainEventPublisher();
        await using var ctx = TestDbContext.CreateInMemory(Guid.NewGuid().ToString(), publisher);
        await ctx.Database.EnsureCreatedAsync();

        var entity = new DomainEventTestEntity { Name = "EventSource2" };
        entity.RaiseOrderCreated();
        ctx.DomainEventEntities.Add(entity);
        await ctx.SaveChangesAsync();

        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public async Task SaveChangesAsync_DoesNotThrow_WhenNoDomainEventsAndNoPublisher()
    {
        // No publisher, no domain events – should silently succeed.
        var entity = new DomainEventTestEntity { Name = "Quiet" };
        _ctx.DomainEventEntities.Add(entity);

        var ex = await Record.ExceptionAsync(() => _ctx.SaveChangesAsync());

        Assert.Null(ex);
    }

    // ── Entity not found ──────────────────────────────────────────────────────

    [Fact]
    public async Task FindAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        var missing = await _ctx.AuditableEntities.FindAsync(Guid.NewGuid());

        Assert.Null(missing);
    }

    // ── Outbox DbSet ──────────────────────────────────────────────────────────

    [Fact]
    public async Task OutboxMessages_CanPersistAndQuery_ViaDbSet()
    {
        var msg = new OutboxMessage
        {
            EventType = "test",
            Payload = "{}",
            Topic = "t",
        };
        _ctx.OutboxMessages.Add(msg);
        await _ctx.SaveChangesAsync();

        var loaded = await _ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
        Assert.NotNull(loaded);
    }

    public void Dispose() => _ctx.Dispose();
}

/// <summary>Test double for <see cref="IDomainEventPublisher"/>.</summary>
internal sealed class FakeDomainEventPublisher : IDomainEventPublisher
{
    public List<IDomainEvent> Published { get; } = [];

    public Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default)
        where T : IDomainEvent
    {
        Published.Add(domainEvent);
        return Task.CompletedTask;
    }

    public Task PublishAllAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        Published.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}
