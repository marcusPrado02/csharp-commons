using MarcusPrado.Platform.BackgroundJobs.Abstractions;
using Microsoft.Extensions.Logging;
using Quartz;
using PlatformIJob = MarcusPrado.Platform.BackgroundJobs.Abstractions.IJob;

namespace MarcusPrado.Platform.Quartz;

/// <summary>
/// Implementation of <see cref="IJobScheduler"/> that delegates to a Quartz.NET <see cref="IScheduler"/>.
/// </summary>
public sealed partial class QuartzJobScheduler : IJobScheduler
{
    private readonly IScheduler _scheduler;
    private readonly ILogger<QuartzJobScheduler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="QuartzJobScheduler"/>.
    /// </summary>
    /// <param name="scheduler">The underlying Quartz scheduler.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public QuartzJobScheduler(IScheduler scheduler, ILogger<QuartzJobScheduler> logger)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task ScheduleAsync<TJob>(string jobKey, JobTrigger trigger, CancellationToken ct = default)
        where TJob : PlatformIJob
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobKey);
        ArgumentNullException.ThrowIfNull(trigger);

        var quartzJobType = typeof(QuartzJobAdapter<TJob>);

        var jobDetail = JobBuilder.Create(quartzJobType)
            .WithIdentity(jobKey)
            .Build();

        var quartzTrigger = BuildQuartzTrigger(jobKey, trigger);

        LogScheduling(_logger, jobKey, typeof(TJob).Name);
        await _scheduler.ScheduleJob(jobDetail, quartzTrigger, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UnscheduleAsync(string jobKey, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobKey);
        LogUnscheduling(_logger, jobKey);
        await _scheduler.DeleteJob(new JobKey(jobKey), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task PauseAsync(string jobKey, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobKey);
        LogPausing(_logger, jobKey);
        await _scheduler.PauseJob(new JobKey(jobKey), ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ResumeAsync(string jobKey, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobKey);
        LogResuming(_logger, jobKey);
        await _scheduler.ResumeJob(new JobKey(jobKey), ct).ConfigureAwait(false);
    }

    private static ITrigger BuildQuartzTrigger(string jobKey, JobTrigger trigger)
    {
        var triggerBuilder = TriggerBuilder.Create()
            .WithIdentity($"{jobKey}-trigger")
            .ForJob(jobKey);

        triggerBuilder = trigger.StartAt.HasValue
            ? triggerBuilder.StartAt(trigger.StartAt.Value)
            : triggerBuilder.StartNow();

        if (!string.IsNullOrWhiteSpace(trigger.CronExpression))
        {
            triggerBuilder = triggerBuilder.WithCronSchedule(trigger.CronExpression);
        }
        else if (trigger.RepeatInterval.HasValue)
        {
            var interval = trigger.RepeatInterval.Value;
            var count = trigger.RepeatCount;
            triggerBuilder = triggerBuilder.WithSimpleSchedule(x =>
            {
                x.WithInterval(interval);
                if (count.HasValue)
                {
                    x.WithRepeatCount(count.Value);
                }
                else
                {
                    x.RepeatForever();
                }
            });
        }
        else
        {
            triggerBuilder = triggerBuilder.WithSimpleSchedule(x => x.WithRepeatCount(0));
        }

        return triggerBuilder.Build();
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Scheduling job '{JobKey}' of type '{JobType}'.")]
    private static partial void LogScheduling(ILogger logger, string jobKey, string jobType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Unscheduling job '{JobKey}'.")]
    private static partial void LogUnscheduling(ILogger logger, string jobKey);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Pausing job '{JobKey}'.")]
    private static partial void LogPausing(ILogger logger, string jobKey);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Resuming job '{JobKey}'.")]
    private static partial void LogResuming(ILogger logger, string jobKey);
}
