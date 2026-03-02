namespace MarcusPrado.Platform.TestKit.Tests.Fakes;

public sealed class FakeClockTests
{
    [Fact]
    public void DefaultCtor_UtcNow_IsCloseToNow()
    {
        var clock = new FakeClock();
        var diff = DateTimeOffset.UtcNow - clock.UtcNow;
        Assert.True(Math.Abs(diff.TotalSeconds) < 5);
    }

    [Fact]
    public void SetNow_UpdatesUtcNow()
    {
        var clock = new FakeClock();
        var target = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        clock.SetNow(target);
        Assert.Equal(target, clock.UtcNow);
    }

    [Fact]
    public void Advance_MovesClockForward()
    {
        var start = new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var clock = new FakeClock(start);
        clock.Advance(TimeSpan.FromHours(1));
        Assert.Equal(start.AddHours(1), clock.UtcNow);
    }

    [Fact]
    public void Ctor_WithStartTime_UsesGivenTime()
    {
        var t = new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero);
        var clock = new FakeClock(t);
        Assert.Equal(t, clock.UtcNow);
    }
}
