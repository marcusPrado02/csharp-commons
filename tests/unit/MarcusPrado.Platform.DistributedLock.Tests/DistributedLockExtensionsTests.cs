namespace MarcusPrado.Platform.DistributedLock.Tests;

public sealed class DistributedLockExtensionsTests
{
    [Fact]
    public async Task WithLockAsync_WhenAcquired_ExecutesAction()
    {
        var executed = false;
        var mockHandle = Substitute.For<IAsyncDisposable>();
        var mockLock = Substitute.For<IDistributedLock>();

        mockLock.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IAsyncDisposable?>(mockHandle));

        await mockLock.WithLockAsync("key", TimeSpan.FromSeconds(5), async () =>
        {
            executed = true;
            await Task.CompletedTask;
        });

        executed.Should().BeTrue();
        await mockHandle.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task WithLockAsync_WhenNotAcquired_ThrowsInvalidOperationException()
    {
        var mockLock = Substitute.For<IDistributedLock>();
        mockLock.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IAsyncDisposable?>(null));

        var act = () => mockLock.WithLockAsync("busy-key", TimeSpan.FromSeconds(5), () => Task.CompletedTask);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*busy-key*");
    }

    [Fact]
    public async Task WithLockAsync_ReturnsResultFromFunc()
    {
        var mockHandle = Substitute.For<IAsyncDisposable>();
        var mockLock = Substitute.For<IDistributedLock>();

        mockLock.AcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IAsyncDisposable?>(mockHandle));

        var result = await mockLock.WithLockAsync(
            "key", TimeSpan.FromSeconds(5), () => Task.FromResult(42));

        result.Should().Be(42);
        await mockHandle.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task WithLockAsync_NullAction_ThrowsArgumentNullException()
    {
        var mockLock = Substitute.For<IDistributedLock>();

        var act = () => mockLock.WithLockAsync("key", TimeSpan.FromSeconds(5), (Func<Task>)null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
