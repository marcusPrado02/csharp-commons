namespace MarcusPrado.Platform.OutboxInbox.Tests.Idempotency;

public sealed class InMemoryIdempotencyStoreTests
{
    private readonly InMemoryIdempotencyStore _store = new();

    [Fact]
    public async Task SetAndExists_ReturnsTrue()
    {
        var key = new IdempotencyKey("op:1");
        await _store.SetAsync(new IdempotencyRecord { Key = key.Value });

        var exists = await _store.ExistsAsync(key);

        Assert.True(exists);
    }

    [Fact]
    public async Task NonExistentKey_ReturnsFalse()
    {
        var key = new IdempotencyKey("op:2");

        var exists = await _store.ExistsAsync(key);

        Assert.False(exists);
    }

    [Fact]
    public async Task ExpiredRecord_IsIgnored()
    {
        var key = new IdempotencyKey("op:3");
        await _store.SetAsync(new IdempotencyRecord
        {
            Key = key.Value,
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1),
        });

        var exists = await _store.ExistsAsync(key);

        Assert.False(exists);
    }

    [Fact]
    public async Task Remove_DeletesRecord()
    {
        var key = new IdempotencyKey("op:4");
        await _store.SetAsync(new IdempotencyRecord { Key = key.Value });

        await _store.RemoveAsync(key);

        Assert.False(await _store.ExistsAsync(key));
    }

    [Fact]
    public async Task Get_ReturnsRecord()
    {
        var key = new IdempotencyKey("op:5");
        var record = new IdempotencyRecord { Key = key.Value, ResultPayload = "ok" };
        await _store.SetAsync(record);

        var retrieved = await _store.GetAsync(key);

        Assert.NotNull(retrieved);
        Assert.Equal("ok", retrieved.ResultPayload);
    }
}
