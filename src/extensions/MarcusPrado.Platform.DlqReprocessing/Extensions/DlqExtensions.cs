using MarcusPrado.Platform.DlqReprocessing.Jobs;

namespace MarcusPrado.Platform.DlqReprocessing.Extensions;

/// <summary>
/// Extension methods for registering the DLQ reprocessing subsystem into the DI container.
/// </summary>
public static class DlqExtensions
{
    /// <summary>
    /// Registers all DLQ reprocessing services:
    /// <list type="bullet">
    /// <item><see cref="IDlqStore"/> backed by <see cref="InMemoryDlqStore"/> (singleton).</item>
    /// <item><see cref="IDlqMetrics"/> backed by <see cref="OtelDlqMetrics"/> (singleton).</item>
    /// <item><see cref="DlqReprocessingJob"/> as a hosted <see cref="BackgroundService"/>.</item>
    /// <item><see cref="DlqOptions"/> configuration.</item>
    /// </list>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">Optional delegate to configure <see cref="DlqOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddPlatformDlqReprocessing(
        this IServiceCollection services,
        Action<DlqOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<DlqOptions>();
        if (configure is not null)
            optionsBuilder.Configure(configure);

        services.AddSingleton<IDlqStore, InMemoryDlqStore>();
        services.AddSingleton<IDlqMetrics, OtelDlqMetrics>();
        services.AddHostedService<DlqReprocessingJob>();

        return services;
    }
}
