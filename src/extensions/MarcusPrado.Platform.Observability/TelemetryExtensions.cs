using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace MarcusPrado.Platform.Observability;

public static class TelemetryExtensions
{
    private const string DataAccessActivitySource = "MarcusPrado.Platform.DataAccess";

    public static IServiceCollection AddPlatformTelemetry(
        this IServiceCollection services,
        Action<TelemetryOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var opts = new TelemetryOptions();
        configure?.Invoke(opts);
        services.AddSingleton(opts);
        services.AddSingleton(sp => new PlatformMeter(opts.ServiceName));

        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(serviceName: opts.ServiceName, serviceVersion: opts.ServiceVersion);

        var otelBuilder = services.AddOpenTelemetry();

        if (opts.EnableTracing)
        {
            otelBuilder.WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(resourceBuilder);
                if (opts.EnableAspNetCoreInstrumentation)
                    tracing.AddAspNetCoreInstrumentation();
                if (opts.EnableHttpClientInstrumentation)
                    tracing.AddHttpClientInstrumentation();
                tracing.AddSource(DataAccessActivitySource);
                tracing.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(opts.OtlpEndpoint));
            });
        }

        if (opts.EnableMetrics)
        {
            otelBuilder.WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(resourceBuilder);
                metrics.AddMeter(PlatformMeter.MeterName);
                if (opts.EnableAspNetCoreInstrumentation)
                    metrics.AddAspNetCoreInstrumentation();
                if (opts.EnableHttpClientInstrumentation)
                    metrics.AddHttpClientInstrumentation();
                if (opts.EnableRuntimeMetrics)
                    metrics.AddRuntimeInstrumentation();
                metrics.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(opts.OtlpEndpoint));
            });
        }

        if (opts.EnableLogs)
        {
            services.AddLogging(logging =>
            {
                logging.AddOpenTelemetry(otlpLogs =>
                {
                    otlpLogs.SetResourceBuilder(resourceBuilder);
                    otlpLogs.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(opts.OtlpEndpoint));
                });
            });
        }

        services.AddSingleton<IHealthCheckPublisher, OtelHealthCheckPublisher>();

        return services;
    }
}
