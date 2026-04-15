using FluentAssertions;
using MarcusPrado.Platform.Redis.Caching;
using MarcusPrado.Platform.Redis.Lock;
using NSubstitute;

namespace MarcusPrado.Platform.Redis.Tests.Caching;

public sealed class StampedeProtectedCacheTests
{
    private readonly ICache _inner = Substitute.For<ICache>();
    private readonly IDistributedLock _lock = Substitute.For<IDistributedLock>();
    private readonly ILockHandle _handle = Substitute.For<ILockHandle>();

    public StampedeProtectedCacheTests()
    {
        _lock
            .AcquireAsync(
                Arg.Any<string>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<int>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(_handle));
    }

    [Fact]
    public async Task GetAsync_DelegatesToInner()
    {
        _inner
            .GetAsync<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>("val"));

        var sut = new StampedeProtectedCache(_inner, _lock);
        var result = await sut.GetAsync<string>("k");

        result.Should().Be("val");
    }

    [Fact]
    public async Task SetAsync_DelegatesToInner()
    {
        var sut = new StampedeProtectedCache(_inner, _lock);
        await sut.SetAsync("k", "v");

        await _inner.Received(1).SetAsync("k", "v", Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_DelegatesToInner()
    {
        var sut = new StampedeProtectedCache(_inner, _lock);
        await sut.RemoveAsync("k");

        await _inner.Received(1).RemoveAsync("k", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExistsAsync_DelegatesToInner()
    {
        _inner.ExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

        var sut = new StampedeProtectedCache(_inner, _lock);
        var result = await sut.ExistsAsync("k");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrAddAsync_CacheHit_DoesNotCallFactory()
    {
        _inner
            .GetAsync<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>("cached"));

        var sut = new StampedeProtectedCache(_inner, _lock);
        var called = false;

        var result = await sut.GetOrAddAsync<string>(
            "k",
            async _ =>
            {
                called = true;
                return await Task.FromResult("miss");
            }
        );

        result.Should().Be("cached");
        called.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrAddAsync_CacheMiss_CallsFactoryAndCachesResult()
    {
        _inner
            .GetAsync<string>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<string?>(null));

        var sut = new StampedeProtectedCache(_inner, _lock);
        var called = false;

        var result = await sut.GetOrAddAsync<string>(
            "k",
            async _ =>
            {
                called = true;
                return await Task.FromResult<string?>("fresh");
            }
        );

        result.Should().Be("fresh");
        called.Should().BeTrue();
        await _inner.Received(1).SetAsync("k", "fresh", Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }
}
