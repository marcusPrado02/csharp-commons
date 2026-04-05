namespace MarcusPrado.Platform.Hangfire.Options;

/// <summary>Options for configuring the Hangfire platform integration.</summary>
public sealed class HangfireOptions
{
    /// <summary>
    /// Gets or sets the path prefix for the Hangfire dashboard.
    /// Defaults to <c>"/hangfire"</c>.
    /// </summary>
    public string DashboardPath { get; set; } = "/hangfire";

    /// <summary>
    /// Gets or sets the number of background-job server worker threads.
    /// Defaults to <c>5</c>.
    /// </summary>
    public int WorkerCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets the queues processed by the background-job server.
    /// Defaults to <c>["default"]</c>.
    /// </summary>
    public string[] Queues { get; set; } = ["default"];
}
