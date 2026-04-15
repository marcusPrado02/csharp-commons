namespace MarcusPrado.Platform.TestKit.Tests.Helpers;

public sealed class EventuallyTests
{
    [Fact]
    public async Task BecomesTrue_Sync_PassesImmediately()
    {
        var act = async () => await Eventually.BecomesTrue(() => true);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task BecomesTrue_Sync_ThrowsOnTimeout()
    {
        await Assert.ThrowsAsync<TimeoutException>(() =>
            Eventually.BecomesTrue(
                () => false,
                timeout: TimeSpan.FromMilliseconds(100),
                interval: TimeSpan.FromMilliseconds(20)
            )
        );
    }

    [Fact]
    public async Task BecomesTrue_Async_PassesWhenTrue()
    {
        var act = async () => await Eventually.BecomesTrue(() => Task.FromResult(true));
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task BecomesTrue_Async_ThrowsOnTimeout()
    {
        await Assert.ThrowsAsync<TimeoutException>(() =>
            Eventually.BecomesTrue(
                () => Task.FromResult(false),
                timeout: TimeSpan.FromMilliseconds(100),
                interval: TimeSpan.FromMilliseconds(20)
            )
        );
    }
}
