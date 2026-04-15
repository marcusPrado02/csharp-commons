using MarcusPrado.Platform.BackgroundJobs.Abstractions;

namespace MarcusPrado.Platform.Quartz;

/// <summary>
/// Fluent builder for constructing <see cref="JobTrigger"/> instances.
/// </summary>
public sealed class JobTriggerBuilder
{
    private string? _cronExpression;
    private TimeSpan? _repeatInterval;
    private DateTimeOffset? _startAt;
    private int? _repeatCount;

    /// <summary>
    /// Configures the trigger to fire on the given Quartz-compatible cron expression.
    /// </summary>
    /// <param name="cronExpression">The cron expression (e.g. "0 0 * * * ?").</param>
    /// <returns>This builder instance.</returns>
    public JobTriggerBuilder WithCron(string cronExpression)
    {
        _cronExpression = cronExpression ?? throw new ArgumentNullException(nameof(cronExpression));
        return this;
    }

    /// <summary>
    /// Configures the trigger to repeat at the given interval.
    /// </summary>
    /// <param name="interval">The interval between executions.</param>
    /// <returns>This builder instance.</returns>
    public JobTriggerBuilder WithRepeat(TimeSpan interval)
    {
        _repeatInterval = interval;
        return this;
    }

    /// <summary>
    /// Configures the trigger to fire a fixed number of additional times after the first execution.
    /// </summary>
    /// <param name="count">Number of additional repeats.</param>
    /// <returns>This builder instance.</returns>
    public JobTriggerBuilder WithRepeatCount(int count)
    {
        _repeatCount = count;
        return this;
    }

    /// <summary>
    /// Configures the trigger to start immediately (i.e. <see cref="DateTimeOffset.UtcNow"/>).
    /// </summary>
    /// <returns>This builder instance.</returns>
    public JobTriggerBuilder StartNow()
    {
        _startAt = DateTimeOffset.UtcNow;
        return this;
    }

    /// <summary>
    /// Configures the trigger to start at the given point in time.
    /// </summary>
    /// <param name="startAt">The desired start time.</param>
    /// <returns>This builder instance.</returns>
    public JobTriggerBuilder StartAt(DateTimeOffset startAt)
    {
        _startAt = startAt;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured <see cref="JobTrigger"/>.
    /// </summary>
    /// <returns>A new <see cref="JobTrigger"/> based on the current builder state.</returns>
    public JobTrigger Build() => new(_cronExpression, _repeatInterval, _startAt, _repeatCount);
}
