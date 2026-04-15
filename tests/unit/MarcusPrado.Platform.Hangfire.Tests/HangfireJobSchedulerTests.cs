using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using MarcusPrado.Platform.BackgroundJobs.Abstractions;
using MarcusPrado.Platform.Hangfire.Scheduler;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace MarcusPrado.Platform.Hangfire.Tests;

public sealed class HangfireJobSchedulerTests
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly HangfireJobScheduler _sut;

    public HangfireJobSchedulerTests()
    {
        _jobClient = Substitute.For<IBackgroundJobClient>();
        _recurringJobManager = Substitute.For<IRecurringJobManager>();
        _sut = new HangfireJobScheduler(_jobClient, _recurringJobManager, NullLogger<HangfireJobScheduler>.Instance);
    }

    [Fact]
    public void Constructor_NullJobClient_ThrowsArgumentNullException()
    {
        var act = () =>
            new HangfireJobScheduler(null!, _recurringJobManager, NullLogger<HangfireJobScheduler>.Instance);

        act.Should().Throw<ArgumentNullException>().WithParameterName("jobClient");
    }

    [Fact]
    public void Constructor_NullRecurringJobManager_ThrowsArgumentNullException()
    {
        var act = () => new HangfireJobScheduler(_jobClient, null!, NullLogger<HangfireJobScheduler>.Instance);

        act.Should().Throw<ArgumentNullException>().WithParameterName("recurringJobManager");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new HangfireJobScheduler(_jobClient, _recurringJobManager, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Enqueue_CallsBackgroundJobClientCreate()
    {
        // IBackgroundJobClient.Enqueue<T> is an extension method that calls .Create(job, state).
        // We verify the underlying Create() is called with a job for SampleJob.
        _jobClient.Create(Arg.Any<Job>(), Arg.Any<IState>()).Returns("job-123");

        var result = _sut.Enqueue<SampleJob>();

        result.Should().Be("job-123");
        _jobClient.Received(1).Create(Arg.Is<Job>(j => j.Type == typeof(SampleJob)), Arg.Any<IState>());
    }

    [Fact]
    public void Schedule_CallsBackgroundJobClientCreate()
    {
        var delay = TimeSpan.FromMinutes(5);
        _jobClient.Create(Arg.Any<Job>(), Arg.Any<IState>()).Returns("job-delayed");

        var result = _sut.Schedule<SampleJob>(delay);

        result.Should().Be("job-delayed");
        _jobClient
            .Received(1)
            .Create(Arg.Is<Job>(j => j.Type == typeof(SampleJob)), Arg.Is<IState>(s => s is ScheduledState));
    }

    [Fact]
    public void AddOrUpdateRecurring_CallsRecurringJobManagerAddOrUpdate()
    {
        // RecurringJobManagerExtensions.AddOrUpdate<T> delegates to IRecurringJobManager.AddOrUpdate(id, job, cron, opts)
        _sut.AddOrUpdateRecurring<SampleJob>("my-job", "*/5 * * * *");

        _recurringJobManager
            .Received(1)
            .AddOrUpdate(
                "my-job",
                Arg.Is<Job>(j => j.Type == typeof(SampleJob)),
                "*/5 * * * *",
                Arg.Any<RecurringJobOptions>()
            );
    }

    [Fact]
    public void RemoveRecurring_CallsRemoveIfExists()
    {
        _sut.RemoveRecurring("my-job");

        _recurringJobManager.Received(1).RemoveIfExists("my-job");
    }

    [Fact]
    public void TriggerRecurring_CallsTrigger()
    {
        _sut.TriggerRecurring("my-job");

        _recurringJobManager.Received(1).Trigger("my-job");
    }

    [Fact]
    public void HangfireJobScheduler_ImplementsIJobScheduler()
    {
        _sut.Should().BeAssignableTo<IJobScheduler>();
    }

    // ── helpers ─────────────────────────────────────────────────────────────────

    private sealed class SampleJob : IHangfireJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
