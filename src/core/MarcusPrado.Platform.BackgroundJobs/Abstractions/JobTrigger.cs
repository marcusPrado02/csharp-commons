namespace MarcusPrado.Platform.BackgroundJobs.Abstractions;

/// <summary>
/// Describes when and how often a background job should fire.
/// </summary>
/// <param name="CronExpression">A Quartz-compatible cron expression (e.g. "0 0 * * * ?").</param>
/// <param name="RepeatInterval">Interval between executions for simple/repeating triggers.</param>
/// <param name="StartAt">Optional point in time at which the trigger should first fire.</param>
/// <param name="RepeatCount">Number of times to repeat after the first execution. Null means repeat indefinitely.</param>
public sealed record JobTrigger(
    string? CronExpression = null,
    TimeSpan? RepeatInterval = null,
    DateTimeOffset? StartAt = null,
    int? RepeatCount = null
);
