using MarcusPrado.Platform.BackgroundJobs.Abstractions;
using Quartz;
using PlatformIJob = MarcusPrado.Platform.BackgroundJobs.Abstractions.IJob;

namespace MarcusPrado.Platform.Quartz;

/// <summary>
/// Quartz <see cref="global::Quartz.IJob"/> adapter that bridges a platform
/// <typeparamref name="TJob"/> into the Quartz execution pipeline.
/// </summary>
/// <typeparam name="TJob">The platform job type to execute.</typeparam>
[DisallowConcurrentExecution]
public sealed class QuartzJobAdapter<TJob> : global::Quartz.IJob
    where TJob : PlatformIJob
{
    /// <summary>Gets the platform job type this adapter wraps.</summary>
    public static Type JobType { get; } = typeof(TJob);

    /// <summary>
    /// Initializes a new instance of <see cref="QuartzJobAdapter{TJob}"/>.
    /// </summary>
    public QuartzJobAdapter()
    {
    }

    /// <inheritdoc />
    public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
}
