using FluentAssertions;
using MarcusPrado.Platform.BackgroundJobs.Abstractions;
using MarcusPrado.Platform.Quartz;
using Xunit;

namespace MarcusPrado.Platform.Quartz.Tests;

public sealed class JobTriggerBuilderTests
{
    [Fact]
    public void Build_DefaultBuilder_ReturnsEmptyTrigger()
    {
        var trigger = new JobTriggerBuilder().Build();

        trigger.CronExpression.Should().BeNull();
        trigger.RepeatInterval.Should().BeNull();
        trigger.StartAt.Should().BeNull();
        trigger.RepeatCount.Should().BeNull();
    }

    [Fact]
    public void WithCron_SetsCronExpression()
    {
        const string cron = "0 0 * * * ?";
        var trigger = new JobTriggerBuilder().WithCron(cron).Build();

        trigger.CronExpression.Should().Be(cron);
    }

    [Fact]
    public void WithRepeat_SetsRepeatInterval()
    {
        var interval = TimeSpan.FromMinutes(5);
        var trigger = new JobTriggerBuilder().WithRepeat(interval).Build();

        trigger.RepeatInterval.Should().Be(interval);
    }

    [Fact]
    public void WithRepeatCount_SetsRepeatCount()
    {
        var trigger = new JobTriggerBuilder().WithRepeatCount(3).Build();

        trigger.RepeatCount.Should().Be(3);
    }

    [Fact]
    public void StartNow_SetsStartAtToNearUtcNow()
    {
        var before = DateTimeOffset.UtcNow;
        var trigger = new JobTriggerBuilder().StartNow().Build();
        var after = DateTimeOffset.UtcNow;

        trigger.StartAt.Should().NotBeNull();
        trigger.StartAt.Should().BeOnOrAfter(before);
        trigger.StartAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void StartAt_SetsExplicitStartTime()
    {
        var startTime = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var trigger = new JobTriggerBuilder().StartAt(startTime).Build();

        trigger.StartAt.Should().Be(startTime);
    }

    [Fact]
    public void FluentChain_CombinesAllProperties()
    {
        var interval = TimeSpan.FromSeconds(30);
        var startAt = DateTimeOffset.UtcNow.AddHours(1);
        var trigger = new JobTriggerBuilder()
            .WithRepeat(interval)
            .WithRepeatCount(10)
            .StartAt(startAt)
            .Build();

        trigger.RepeatInterval.Should().Be(interval);
        trigger.RepeatCount.Should().Be(10);
        trigger.StartAt.Should().Be(startAt);
        trigger.CronExpression.Should().BeNull();
    }

    [Fact]
    public void WithCron_NullArgument_ThrowsArgumentNullException()
    {
        var builder = new JobTriggerBuilder();
        var act = () => builder.WithCron(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
