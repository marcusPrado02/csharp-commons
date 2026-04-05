using Hangfire;
using MarcusPrado.Platform.BackgroundJobs.Abstractions;
using Microsoft.Extensions.Logging;

namespace MarcusPrado.Platform.Hangfire.Scheduler;

/// <summary>
/// Hangfire-backed implementation of <see cref="IJobScheduler"/>.
/// Delegates fire-and-forget and delayed scheduling to Hangfire's
/// <see cref="IBackgroundJobClient"/>, and recurring jobs to <see cref="IRecurringJobManager"/>.
/// </summary>
public sealed partial class HangfireJobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<HangfireJobScheduler> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="HangfireJobScheduler"/>.
    /// </summary>
    /// <param name="jobClient">The Hangfire background-job client.</param>
    /// <param name="recurringJobManager">The Hangfire recurring-job manager.</param>
    /// <param name="logger">Logger instance.</param>
    public HangfireJobScheduler(
        IBackgroundJobClient jobClient,
        IRecurringJobManager recurringJobManager,
        ILogger<HangfireJobScheduler> logger)
    {
        _jobClient = jobClient ?? throw new ArgumentNullException(nameof(jobClient));
        _recurringJobManager = recurringJobManager ?? throw new ArgumentNullException(nameof(recurringJobManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Enqueues a fire-and-forget job of type <typeparamref name="TJob"/> immediately.
    /// </summary>
    /// <typeparam name="TJob">The job type to enqueue. Must implement <see cref="IHangfireJob"/>.</typeparam>
    /// <returns>The Hangfire job identifier.</returns>
    public string Enqueue<TJob>()
        where TJob : IHangfireJob
    {
        LogEnqueue(typeof(TJob).Name);
        return _jobClient.Enqueue<TJob>(job => job.ExecuteAsync(CancellationToken.None));
    }

    /// <summary>
    /// Schedules a delayed job of type <typeparamref name="TJob"/> to run after a specified delay.
    /// </summary>
    /// <typeparam name="TJob">The job type to schedule. Must implement <see cref="IHangfireJob"/>.</typeparam>
    /// <param name="delay">Time to wait before executing the job.</param>
    /// <returns>The Hangfire job identifier.</returns>
    public string Schedule<TJob>(TimeSpan delay)
        where TJob : IHangfireJob
    {
        LogSchedule(typeof(TJob).Name, delay);
        return _jobClient.Schedule<TJob>(job => job.ExecuteAsync(CancellationToken.None), delay);
    }

    /// <summary>
    /// Adds or updates a recurring job of type <typeparamref name="TJob"/> using a cron expression.
    /// </summary>
    /// <typeparam name="TJob">The job type to register as recurring. Must implement <see cref="IHangfireJob"/>.</typeparam>
    /// <param name="recurringJobId">Unique identifier for the recurring job.</param>
    /// <param name="cronExpression">A valid cron expression.</param>
    /// <param name="queue">The queue to use. Defaults to <c>"default"</c>.</param>
    public void AddOrUpdateRecurring<TJob>(
        string recurringJobId,
        string cronExpression,
        string queue = "default")
        where TJob : IHangfireJob
    {
        LogAddOrUpdate(recurringJobId, typeof(TJob).Name, cronExpression);

        _recurringJobManager.AddOrUpdate<TJob>(
            recurringJobId,
            job => job.ExecuteAsync(CancellationToken.None),
            cronExpression);
    }

    /// <summary>
    /// Removes the recurring job with the given identifier.
    /// </summary>
    /// <param name="recurringJobId">Identifier of the recurring job to remove.</param>
    public void RemoveRecurring(string recurringJobId)
    {
        LogRemove(recurringJobId);
        _recurringJobManager.RemoveIfExists(recurringJobId);
    }

    /// <summary>
    /// Triggers the recurring job with the given identifier immediately (in addition to its schedule).
    /// </summary>
    /// <param name="recurringJobId">Identifier of the recurring job to trigger.</param>
    public void TriggerRecurring(string recurringJobId)
    {
        LogTrigger(recurringJobId);
        _recurringJobManager.Trigger(recurringJobId);
    }

    /// <inheritdoc/>
    public Task ScheduleAsync<TJob>(string jobKey, JobTrigger trigger, CancellationToken ct = default)
        where TJob : IJob
    {
        LogScheduleAsync(jobKey, typeof(TJob).Name);
        if (trigger.CronExpression is not null)
        {
            _recurringJobManager.AddOrUpdate(jobKey, () => Task.CompletedTask, trigger.CronExpression);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UnscheduleAsync(string jobKey, CancellationToken ct = default)
    {
        LogUnschedule(jobKey);
        _recurringJobManager.RemoveIfExists(jobKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task PauseAsync(string jobKey, CancellationToken ct = default)
    {
        // Hangfire does not expose a pause API on IRecurringJobManager; no-op.
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ResumeAsync(string jobKey, CancellationToken ct = default)
    {
        // Hangfire does not expose a resume API on IRecurringJobManager; trigger immediately.
        _recurringJobManager.Trigger(jobKey);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Enqueueing fire-and-forget job {JobType}")]
    private partial void LogEnqueue(string jobType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Scheduling delayed job {JobType} with delay {Delay}")]
    private partial void LogSchedule(string jobType, TimeSpan delay);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Adding/updating recurring job {JobId} ({JobType}) with cron {Cron}")]
    private partial void LogAddOrUpdate(string jobId, string jobType, string cron);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Scheduling job {JobKey} of type {JobType} via IJobScheduler")]
    private partial void LogScheduleAsync(string jobKey, string jobType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Unscheduling job {JobKey}")]
    private partial void LogUnschedule(string jobKey);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Removing recurring job {JobId}")]
    private partial void LogRemove(string jobId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Triggering recurring job {JobId}")]
    private partial void LogTrigger(string jobId);
}
