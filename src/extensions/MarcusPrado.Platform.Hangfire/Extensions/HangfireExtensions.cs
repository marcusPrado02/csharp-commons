using Hangfire;
using Hangfire.InMemory;
using MarcusPrado.Platform.BackgroundJobs.Abstractions;
using MarcusPrado.Platform.Hangfire.Options;
using MarcusPrado.Platform.Hangfire.Registrar;
using MarcusPrado.Platform.Hangfire.Scheduler;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Hangfire.Extensions;

/// <summary>
/// Extension methods to integrate the Hangfire platform adapter into an ASP.NET Core application.
/// </summary>
public static class HangfireExtensions
{
    /// <summary>
    /// Registers Hangfire services, configures in-memory storage, and binds
    /// <see cref="IJobScheduler"/> to <see cref="HangfireJobScheduler"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Optional delegate to customise <see cref="HangfireOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddPlatformHangfire(
        this IServiceCollection services,
        Action<HangfireOptions>? configure = null)
    {
        var options = new HangfireOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseInMemoryStorage();
        });

        services.AddHangfireServer(serverOptions =>
        {
            serverOptions.WorkerCount = options.WorkerCount;
            serverOptions.Queues = options.Queues;
        });

        services.AddSingleton<IJobScheduler, HangfireJobScheduler>();
        services.AddSingleton<HangfireJobScheduler>();
        services.AddSingleton<HangfireRecurringJobRegistrar>();

        return services;
    }

    /// <summary>
    /// Adds the Hangfire Dashboard middleware to the request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static IApplicationBuilder UsePlatformHangfire(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<HangfireOptions>();
        app.UseHangfireDashboard(options.DashboardPath);
        return app;
    }
}
