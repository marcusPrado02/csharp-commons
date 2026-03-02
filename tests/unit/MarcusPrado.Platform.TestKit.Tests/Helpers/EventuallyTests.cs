namespace MarcusPrado.Platform.TestKit.Tests.Helpers;

public sealed class EventuallyTests
{
    [Fact]
    public async Task BecomesTrue_Sync_PassesImmediately()
    {
        await Eventually.BecomesTrue(() => true);
    }

    [Fact]
    public async Task BecomesTrue_Sync_ThrowsOnTimeout()
    {
        await Assert.ThrowsAsync<TimeoutException>(
            () => Eventually.BecomesTrue(
                () => false,
                timeout: TimeSpan.FromMilliseconds(100),
                interval: TimeSpan.FromMilliseconds(20)));
    }

    [Fact]
    public async Task BecomesTrue_Async_PassesWhenTrue()
    {
        await Eventually.BecomesTrue(
            () => Task.FromResult(true));
    }

    [Fact]
    public async Task BecomesTrue_Async_ThrowsOnTimeout()
    {
        await Assert.ThrowsAsync<TimeoutException>(
            () => Eventually.BecomesTrue(
                () => Task.FromResult(false),
                timeout: TimeSpan.FromMilliseconds(100),
                interval: TimeSpan.FromMilliseconds(20)));
    }
}
