using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace MarcusPrado.Platform.Quartz;

/// <summary>
/// Quartz <see cref="IJobFactory"/> that resolves <see cref="IJob"/> instances from an <see cref="IServiceProvider"/>.
/// This enables full dependency injection support for Quartz jobs.
/// </summary>
public sealed class PlatformJobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="PlatformJobFactory"/>.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider used to resolve job instances.</param>
    public PlatformJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public IJob NewJob(TriggerFiredBundle bundle, global::Quartz.IScheduler scheduler)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        var jobType = bundle.JobDetail.JobType;
        var job = _serviceProvider.GetService(jobType) ?? ActivatorUtilities.CreateInstance(_serviceProvider, jobType);
        return (IJob)job;
    }

    /// <inheritdoc />
    public void ReturnJob(IJob job)
    {
        if (job is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
