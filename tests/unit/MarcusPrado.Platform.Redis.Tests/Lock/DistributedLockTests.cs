using FluentAssertions;
using MarcusPrado.Platform.Redis.Lock;
using NSubstitute;

namespace MarcusPrado.Platform.Redis.Tests.Lock;

public sealed class DistributedLockTests
{
    [Fact]
    public async Task WithLockAsync_ExecutesAction_AndSetsFlag()
    {
        var executed = false;
        var mockLock = Substitute.For<IDistributedLock>();
        var handle = Substitute.For<ILockHandle>();

        mockLock.AcquireAsync(
                    Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<int>(),
                    Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(handle));

        await mockLock.WithLockAsync("key", TimeSpan.FromSeconds(5), async () =>
        {
            executed = true;
            await Task.CompletedTask;
        });

        executed.Should().BeTrue();
        await handle.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task WithLockAsync_ReturnsResultFromFunc()
    {
        var mockLock = Substitute.For<IDistributedLock>();
        var handle = Substitute.For<ILockHandle>();

        mockLock.AcquireAsync(
                    Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<int>(),
                    Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(handle));

        var result = await mockLock.WithLockAsync(
            "key", TimeSpan.FromSeconds(5), () => Task.FromResult(99));

        result.Should().Be(99);
    }

    [Fact]
    public async Task WithLockAsync_NullAction_ThrowsArgumentNullException()
    {
        var mockLock = Substitute.For<IDistributedLock>();

        var act = () => mockLock.WithLockAsync("key", TimeSpan.FromSeconds(5), (Func<Task>)null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void IDistributedLock_HasAcquireAndTryAcquireMethods()
    {
        typeof(IDistributedLock).GetMethod("AcquireAsync").Should().NotBeNull();
        typeof(IDistributedLock).GetMethod("TryAcquireAsync").Should().NotBeNull();
    }

    [Fact]
    public void ILockHandle_HasExpectedMembers()
    {
        var type = typeof(ILockHandle);
        type.GetProperty("Key").Should().NotBeNull();
        type.GetProperty("Token").Should().NotBeNull();
        type.GetProperty("IsHeld").Should().NotBeNull();
    }
}
