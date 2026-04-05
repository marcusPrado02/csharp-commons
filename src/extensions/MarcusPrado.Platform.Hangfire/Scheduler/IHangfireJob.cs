using MarcusPrado.Platform.BackgroundJobs.Abstractions;

namespace MarcusPrado.Platform.Hangfire.Scheduler;

/// <summary>
/// Extends <see cref="IJob"/> with an <see cref="ExecuteAsync"/> entry-point that Hangfire
/// can invoke when the job is dequeued from the background-job server.
/// </summary>
public interface IHangfireJob : IJob
{
    /// <summary>Executes the job body.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken);
}
