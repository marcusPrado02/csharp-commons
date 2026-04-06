namespace MarcusPrado.Platform.EfCore.Tests.Uow;

public sealed class EfUnitOfWorkTests : IDisposable
{
    private readonly TestDbContext _ctx;
    private readonly EfUnitOfWork _uow;

    public EfUnitOfWorkTests()
    {
        _ctx = TestDbContext.CreateInMemory(Guid.NewGuid().ToString());
        _ctx.Database.EnsureCreated();
        _uow = new EfUnitOfWork(_ctx);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsTrackedEntities()
    {
        var msg = new OutboxMessage { EventType = "uow.test", Payload = "{}", Topic = "t" };
        _ctx.OutboxMessages.Add(msg);

        await _uow.SaveChangesAsync();

        var saved = await _ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task CommitAsync_PersistsTrackedEntities()
    {
        var msg = new OutboxMessage { EventType = "uow.commit", Payload = "{}", Topic = "t" };
        _ctx.OutboxMessages.Add(msg);

        // InMemory provider does not support real transactions, but CommitAsync still
        // calls SaveChanges before attempting to commit the (no-op) transaction.
        await _uow.CommitAsync();

        var saved = await _ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task RollbackAsync_DoesNotThrow_WhenNoTransactionIsActive()
    {
        // RollbackAsync is a no-op when no transaction has been opened.
        var ex = await Record.ExceptionAsync(() => _uow.RollbackAsync());

        Assert.Null(ex);
    }

    [Fact]
    public async Task DisposeAsync_DoesNotThrow_WhenNoTransactionIsActive()
    {
        await using var uow = new EfUnitOfWork(_ctx);

        var ex = await Record.ExceptionAsync(() => uow.DisposeAsync().AsTask());

        Assert.Null(ex);
    }

    [Fact]
    public async Task Constructor_ThrowsArgumentNullException_WhenContextIsNull()
    {
        await Task.CompletedTask; // make async for consistency

        var ex = Assert.Throws<ArgumentNullException>(() => new EfUnitOfWork(null!));
        Assert.Equal("context", ex.ParamName);
    }

    public void Dispose() => _ctx.Dispose();
}
