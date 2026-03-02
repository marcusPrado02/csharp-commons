namespace MarcusPrado.Platform.Application.Scheduling;

/// <summary>Schedules one-off and recurring background jobs.</summary>
public interface IScheduler
{
    /// <summary>Enqueues a job to run immediately (fire-and-forget).</summary>
    Task EnqueueAsync<TJob>(CancellationToken cancellationToken = default)
        where TJob : notnull;

    /// <summary>Schedules a job to run at a specific date/time.</summary>
    Task ScheduleAsync<TJob>(DateTimeOffset runAt, CancellationToken cancellationToken = default)
        where TJob : notnull;

    /// <summary>Registers a recurring job with a cron expression.</summary>
    Task RecurAsync<TJob>(string cronExpression, CancellationToken cancellationToken = default)
        where TJob : notnull;
}
