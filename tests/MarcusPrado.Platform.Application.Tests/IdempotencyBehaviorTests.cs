using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Application.Tests;

public sealed class IdempotencyBehaviorTests
{
    [Fact]
    public async Task NoStore_AlwaysCallsNext()
    {
        var behavior = new IdempotencyBehavior<SimpleCommand, Result<string>>();
        var called = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result.Success<string>("hello"));
        };

        var result = await behavior.HandleAsync(new SimpleCommand(), next);

        Assert.True(called);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task NonIdempotentCommand_SkipsStore()
    {
        var store = Substitute.For<IIdempotencyStore>();
        var behavior = new IdempotencyBehavior<SimpleCommand, Result<string>>(store);
        var called = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result.Success<string>("skipped cache"));
        };

        var result = await behavior.HandleAsync(new SimpleCommand(), next);

        Assert.True(called);
        await store.DidNotReceive().TryGetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IdempotentCommand_CacheMiss_CallsNextAndStores()
    {
        var store = Substitute.For<IIdempotencyStore>();
        store.TryGetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(bool, string?)>((false, null)));

        var behavior = new IdempotencyBehavior<IdempotentCommand, Result<string>>(store);

        var command = new IdempotentCommand("data", "key-1");
        var called = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result.Success<string>("fresh"));
        };

        var result = await behavior.HandleAsync(command, next);

        Assert.True(called);
        Assert.True(result.IsSuccess);
        await store.Received(1).SetAsync(
            "key-1",
            Arg.Any<string>(),
            TimeSpan.FromSeconds(60),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IdempotentCommand_CacheHit_ReturnsCachedAndSkipsNext()
    {
        var store = Substitute.For<IIdempotencyStore>();
        // Use the same JSON format that SafeSerialize produces: {"s":true,"v":"cached"}
        var serialized = """{"s":true,"v":"cached"}""";
        store.TryGetAsync("key-2", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(bool, string?)>((true, serialized)));

        var behavior = new IdempotencyBehavior<IdempotentCommand, Result<string>>(store);
        var command = new IdempotentCommand("data", "key-2");
        var called = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result.Success<string>("should not be here"));
        };

        var result = await behavior.HandleAsync(command, next);

        Assert.False(called);
        Assert.True(result.IsSuccess);
        Assert.Equal("cached", result.Value);
    }
}
