using System.Text.Json;
using StackExchange.Redis;

namespace MarcusPrado.Platform.Redis.Tests.Caching;

public sealed class RedisCacheTests
{
    private static RedisCache BuildCache(IDatabase? db = null, string prefix = "pfx:")
    {
        db ??= Substitute.For<IDatabase>();
        var mux = Substitute.For<IConnectionMultiplexer>();
        mux.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);

        var opts = new RedisCacheOptions { ConnectionString = "localhost:6379", KeyPrefix = prefix };

        return new RedisCache(mux, opts);
    }

    [Fact]
    public async Task GetAsync_MissReturnsNull()
    {
        var db = Substitute.For<IDatabase>();
        db.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(RedisValue.Null));
        var cache = BuildCache(db);

        var result = await cache.GetAsync<string>("missing");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_HitDeserializesValue()
    {
        var db = Substitute.For<IDatabase>();
        var payload = JsonSerializer.Serialize("hello");
        db.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromResult(new RedisValue(payload)));
        var cache = BuildCache(db);

        var result = await cache.GetAsync<string>("key1");

        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task SetAsync_CallsStringSetWithPrefixedKey()
    {
        var db = Substitute.For<IDatabase>();
        db.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>()
            )
            .Returns(Task.FromResult(true));
        var cache = BuildCache(db, prefix: "pre:");

        await cache.SetAsync("my-key", "value");

        await db.Received(1)
            .StringSetAsync(
                Arg.Is<RedisKey>(k => k.ToString().StartsWith("pre:")),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>()
            );
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueWhenKeyPresent()
    {
        var db = Substitute.For<IDatabase>();
        db.KeyExistsAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        var cache = BuildCache(db);

        var exists = await cache.ExistsAsync("k");

        Assert.True(exists);
    }

    [Fact]
    public async Task RemoveAsync_CallsKeyDelete()
    {
        var db = Substitute.For<IDatabase>();
        db.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(Task.FromResult(true));
        var cache = BuildCache(db);

        await cache.RemoveAsync("k");

        await db.Received(1).KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>());
    }
}
