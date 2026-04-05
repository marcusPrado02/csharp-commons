namespace MarcusPrado.Platform.BackgroundJobs.Abstractions;

/// <summary>
/// Abstraction for a background job scheduler capable of scheduling, unscheduling, pausing and resuming jobs.
/// </summary>
public interface IJobScheduler
{
    /// <summary>Schedules a job of type <typeparamref name="TJob"/> with the given trigger.</summary>
    /// <typeparam name="TJob">The job type to schedule.</typeparam>
    /// <param name="jobKey">A unique key identifying this job.</param>
    /// <param name="trigger">The trigger configuration for the job.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task ScheduleAsync<TJob>(string jobKey, JobTrigger trigger, CancellationToken ct = default)
        where TJob : IJob;

    /// <summary>Removes a previously scheduled job.</summary>
    /// <param name="jobKey">The unique key of the job to remove.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task UnscheduleAsync(string jobKey, CancellationToken ct = default);

    /// <summary>Pauses a scheduled job so it will not fire until resumed.</summary>
    /// <param name="jobKey">The unique key of the job to pause.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task PauseAsync(string jobKey, CancellationToken ct = default);

    /// <summary>Resumes a previously paused job.</summary>
    /// <param name="jobKey">The unique key of the job to resume.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task ResumeAsync(string jobKey, CancellationToken ct = default);
}
