using MarcusPrado.Platform.BackgroundJobs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace MarcusPrado.Platform.Quartz;

/// <summary>
/// Extension methods for registering Quartz.NET services with the Microsoft DI container.
/// </summary>
public static class QuartzExtensions
{
    /// <summary>
    /// Adds the Quartz.NET background job infrastructure, including the <see cref="IJobScheduler"/>
    /// implementation, <see cref="PlatformJobFactory"/>, and the Quartz hosted service.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Optional action to further configure Quartz options.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddPlatformQuartz(
        this IServiceCollection services,
        Action<QuartzOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register Quartz with DI
        services.AddQuartz(q =>
        {
            q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 10);

            if (configure is not null)
            {
                // Allow caller to further customise via the QuartzOptions action
            }
        });

        if (configure is not null)
        {
            services.Configure(configure);
        }

        // Replace the default Quartz job factory with ours so platform jobs get DI
        services.AddSingleton<IJobFactory, PlatformJobFactory>();

        // Register the hosted service that starts/stops the scheduler
        services.AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });

        // Register our IJobScheduler implementation
        // IScheduler is resolved from the QuartzHostedService / ISchedulerFactory
        services.AddSingleton<IJobScheduler>(sp =>
        {
            var factory = sp.GetRequiredService<ISchedulerFactory>();
            var scheduler = factory.GetScheduler().GetAwaiter().GetResult();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<QuartzJobScheduler>>();
            return new QuartzJobScheduler(scheduler, logger);
        });

        return services;
    }
}
