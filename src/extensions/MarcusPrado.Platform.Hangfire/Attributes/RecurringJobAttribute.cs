namespace MarcusPrado.Platform.Hangfire.Attributes;

/// <summary>
/// Marks a job class so that <see cref="Registrar.HangfireRecurringJobRegistrar"/> will
/// automatically register it as a Hangfire recurring job using the supplied cron expression.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class RecurringJobAttribute : Attribute
{
    /// <summary>Gets the cron expression for the recurring job schedule.</summary>
    public string CronExpression { get; }

    /// <summary>Gets or sets the queue the job should be enqueued on. Defaults to <c>"default"</c>.</summary>
    public string Queue { get; set; } = "default";

    /// <summary>Gets or sets the time zone id used to evaluate the cron expression. Defaults to UTC.</summary>
    public string TimeZoneId { get; set; } = "UTC";

    /// <summary>
    /// Initialises a new instance of <see cref="RecurringJobAttribute"/>.
    /// </summary>
    /// <param name="cronExpression">A valid cron expression, e.g. <c>"*/5 * * * *"</c>.</param>
    public RecurringJobAttribute(string cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
            throw new ArgumentException("Cron expression must not be empty.", nameof(cronExpression));

        CronExpression = cronExpression;
    }
}
