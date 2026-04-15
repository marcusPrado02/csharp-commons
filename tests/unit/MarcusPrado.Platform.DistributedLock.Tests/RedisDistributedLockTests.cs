using NSubstitute.ReturnsExtensions;

namespace MarcusPrado.Platform.DistributedLock.Tests;

public sealed class RedisDistributedLockTests
{
    // Helper: configure IDatabase.StringSetAsync (6-param overload) to return a given value.
    private static void SetupStringSet(IDatabase db, bool returns)
    {
        db.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>()
            )
            .Returns(Task.FromResult(returns));
    }

    [Fact]
    public async Task AcquireAsync_WhenRedisReturnsTrue_ReturnsNonNullHandle()
    {
        var db = Substitute.For<IDatabase>();
        SetupStringSet(db, returns: true);

        var sut = new RedisDistributedLock(db);
        var handle = await sut.AcquireAsync("my-key", TimeSpan.FromSeconds(30));

        handle.Should().NotBeNull();
    }

    [Fact]
    public async Task AcquireAsync_WhenRedisReturnsFalse_ReturnsNull()
    {
        var db = Substitute.For<IDatabase>();
        SetupStringSet(db, returns: false);

        var sut = new RedisDistributedLock(db);
        var handle = await sut.AcquireAsync("my-key", TimeSpan.FromSeconds(30));

        handle.Should().BeNull();
    }

    [Fact]
    public async Task AcquireAsync_UsesFencingToken_MonotonicallyIncreasing()
    {
        // Capture two successive tokens
        RedisValue? first = null;
        RedisValue? second = null;
        var callCount = 0;

        var db = Substitute.For<IDatabase>();
        db.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>()
            )
            .Returns(ci =>
            {
                var val = ci.ArgAt<RedisValue>(1);
                callCount++;
                if (callCount == 1)
                    first = val;
                else
                    second = val;
                return Task.FromResult(true);
            });

        var sut = new RedisDistributedLock(db);
        await sut.AcquireAsync("key-a", TimeSpan.FromSeconds(5));
        await sut.AcquireAsync("key-b", TimeSpan.FromSeconds(5));

        first.Should().NotBeNull();
        second.Should().NotBeNull();
        var firstNum = long.Parse(first!.Value.ToString());
        var secondNum = long.Parse(second!.Value.ToString());
        secondNum.Should().BeGreaterThan(firstNum);
    }

    [Fact]
    public async Task AcquireAsync_PassesCorrectKeyAndExpiry_ToRedis()
    {
        RedisKey? capturedKey = null;
        TimeSpan? capturedExpiry = null;

        var db = Substitute.For<IDatabase>();
        db.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>()
            )
            .Returns(ci =>
            {
                capturedKey = ci.ArgAt<RedisKey>(0);
                capturedExpiry = ci.ArgAt<TimeSpan?>(2);
                return Task.FromResult(true);
            });

        var sut = new RedisDistributedLock(db);
        var expiry = TimeSpan.FromMinutes(2);
        await sut.AcquireAsync("resource-xyz", expiry);

        capturedKey!.Value.ToString().Should().Be("resource-xyz");
        capturedExpiry.Should().Be(expiry);
    }

    [Fact]
    public async Task AcquireAsync_UsesWhenNotExists()
    {
        When? capturedWhen = null;

        var db = Substitute.For<IDatabase>();
        db.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>()
            )
            .Returns(ci =>
            {
                capturedWhen = ci.ArgAt<When>(4);
                return Task.FromResult(true);
            });

        var sut = new RedisDistributedLock(db);
        await sut.AcquireAsync("key", TimeSpan.FromSeconds(10));

        capturedWhen.Should().Be(When.NotExists);
    }

    [Fact]
    public async Task Handle_DisposeAsync_ExecutesLuaReleaseScript()
    {
        var db = Substitute.For<IDatabase>();
        SetupStringSet(db, returns: true);

        db.ScriptEvaluateAsync(
                Arg.Any<string>(),
                Arg.Any<RedisKey[]?>(),
                Arg.Any<RedisValue[]?>(),
                Arg.Any<CommandFlags>()
            )
            .Returns(Task.FromResult(RedisResult.Create(1)));

        var sut = new RedisDistributedLock(db);
        var handle = await sut.AcquireAsync("my-lock", TimeSpan.FromSeconds(30));

        handle.Should().NotBeNull();
        await handle!.DisposeAsync();

        await db.Received(1)
            .ScriptEvaluateAsync(
                Arg.Any<string>(),
                Arg.Any<RedisKey[]?>(),
                Arg.Any<RedisValue[]?>(),
                Arg.Any<CommandFlags>()
            );
    }

    [Fact]
    public async Task Handle_DisposeAsync_CalledTwice_ReleasesOnlyOnce()
    {
        var db = Substitute.For<IDatabase>();
        SetupStringSet(db, returns: true);

        db.ScriptEvaluateAsync(
                Arg.Any<string>(),
                Arg.Any<RedisKey[]?>(),
                Arg.Any<RedisValue[]?>(),
                Arg.Any<CommandFlags>()
            )
            .Returns(Task.FromResult(RedisResult.Create(1)));

        var sut = new RedisDistributedLock(db);
        var handle = await sut.AcquireAsync("my-lock", TimeSpan.FromSeconds(30));

        handle.Should().NotBeNull();
        await handle!.DisposeAsync();
        await handle.DisposeAsync(); // second call should be no-op

        await db.Received(1)
            .ScriptEvaluateAsync(
                Arg.Any<string>(),
                Arg.Any<RedisKey[]?>(),
                Arg.Any<RedisValue[]?>(),
                Arg.Any<CommandFlags>()
            );
    }
}
