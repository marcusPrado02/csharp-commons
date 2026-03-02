namespace MarcusPrado.Platform.Governance.Tests.Deprecation;

public sealed class DeprecationScheduleTests
{
    private static readonly DateTimeOffset DepDate = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset RetDate = new(2025, 9, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly DeprecationSchedule _schedule = new(DepDate, RetDate);

    [Fact]
    public void Constructor_Throws_WhenRetirementDateNotAfterDeprecationDate()
    {
        var act = () => new DeprecationSchedule(RetDate, DepDate);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsActive_ReturnsTrue_BeforeDeprecationDate()
    {
        var before = DepDate.AddDays(-1);

        _schedule.IsActive(before).Should().BeTrue();
    }

    [Fact]
    public void IsWithinDeprecationWindow_ReturnsTrue_AfterDeprecation()
    {
        _schedule.IsWithinDeprecationWindow(DepDate).Should().BeTrue();
    }

    [Fact]
    public void IsWithinDeprecationWindow_ReturnsFalse_AfterRetirement()
    {
        _schedule.IsWithinDeprecationWindow(RetDate).Should().BeFalse();
    }

    [Fact]
    public void IsRetired_ReturnsTrue_OnRetirementDate()
    {
        _schedule.IsRetired(RetDate).Should().BeTrue();
    }

    [Fact]
    public void IsRetired_ReturnsFalse_BeforeRetirementDate()
    {
        _schedule.IsRetired(RetDate.AddDays(-1)).Should().BeFalse();
    }
}
