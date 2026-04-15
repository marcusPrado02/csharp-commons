using FluentAssertions;
using MarcusPrado.Platform.BackgroundJobs.Abstractions;
using MarcusPrado.Platform.Quartz;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Quartz;
using Xunit;
using PlatformIJob = MarcusPrado.Platform.BackgroundJobs.Abstractions.IJob;

namespace MarcusPrado.Platform.Quartz.Tests;

public sealed class QuartzJobSchedulerTests
{
    private readonly IScheduler _mockScheduler;
    private readonly QuartzJobScheduler _sut;

    public QuartzJobSchedulerTests()
    {
        _mockScheduler = Substitute.For<IScheduler>();
        _sut = new QuartzJobScheduler(_mockScheduler, NullLogger<QuartzJobScheduler>.Instance);
    }

    [Fact]
    public async Task ScheduleAsync_CallsSchedulerScheduleJob()
    {
        var trigger = new JobTriggerBuilder().StartNow().Build();

        await _sut.ScheduleAsync<StubJob>("job1", trigger);

        await _mockScheduler
            .Received(1)
            .ScheduleJob(
                Arg.Is<IJobDetail>(j => j.Key.Name == "job1"),
                Arg.Any<ITrigger>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task UnscheduleAsync_CallsDeleteJob()
    {
        await _sut.UnscheduleAsync("job1");

        await _mockScheduler.Received(1).DeleteJob(Arg.Is<JobKey>(k => k.Name == "job1"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PauseAsync_CallsPauseJob()
    {
        await _sut.PauseAsync("job1");

        await _mockScheduler.Received(1).PauseJob(Arg.Is<JobKey>(k => k.Name == "job1"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResumeAsync_CallsResumeJob()
    {
        await _sut.ResumeAsync("job1");

        await _mockScheduler.Received(1).ResumeJob(Arg.Is<JobKey>(k => k.Name == "job1"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScheduleAsync_WithCronTrigger_SchedulesWithCronExpression()
    {
        var trigger = new JobTriggerBuilder().WithCron("0 0 12 * * ?").Build();
        ITrigger? capturedTrigger = null;
        await _mockScheduler.ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Do<ITrigger>(t => capturedTrigger = t),
            Arg.Any<CancellationToken>()
        );

        await _sut.ScheduleAsync<StubJob>("cron-job", trigger);

        capturedTrigger.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullScheduler_ThrowsArgumentNullException()
    {
        var act = () => new QuartzJobScheduler(null!, NullLogger<QuartzJobScheduler>.Instance);
        act.Should().Throw<ArgumentNullException>().WithParameterName("scheduler");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new QuartzJobScheduler(_mockScheduler, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task ScheduleAsync_EmptyJobKey_ThrowsArgumentException()
    {
        var trigger = new JobTriggerBuilder().Build();
        var act = async () => await _sut.ScheduleAsync<StubJob>(string.Empty, trigger);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    private sealed class StubJob : PlatformIJob { }
}
