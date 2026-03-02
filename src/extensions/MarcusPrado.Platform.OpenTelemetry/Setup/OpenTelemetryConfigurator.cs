using MarcusPrado.Platform.OpenTelemetry.Conventions;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MarcusPrado.Platform.OpenTelemetry.Setup;

/// <summary>Configures OpenTelemetry traces and metrics using platform conventions.</summary>
public static class OpenTelemetryConfigurator
{
    /// <summary>
    /// Registers OpenTelemetry tracing and metrics with platform-standard configuration.
    /// </summary>
    public static IServiceCollection AddPlatformOpenTelemetry(
        this IServiceCollection services,
        Action<OpenTelemetryOptions>? configure = null)
    {
        var opts = new OpenTelemetryOptions();
        configure?.Invoke(opts);

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(opts.ServiceName, serviceVersion: opts.ServiceVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                [PlatformSpanAttributes.Environment] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown",
            });

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(resourceBuilder);

                if (opts.InstrumentAspNetCore)
                {
                    tracing.AddAspNetCoreInstrumentation(o =>
                    {
                        o.RecordException = true;
                    });
                }

                if (opts.InstrumentHttpClient)
                {
                    tracing.AddHttpClientInstrumentation();
                }

                foreach (var source in opts.AdditionalActivitySources)
                {
                    tracing.AddSource(source);
                }

                if (opts.UseOtlpExporter)
                {
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(opts.OtlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(resourceBuilder);

                if (opts.InstrumentAspNetCore)
                {
                    metrics.AddAspNetCoreInstrumentation();
                }

                if (opts.InstrumentHttpClient)
                {
                    metrics.AddHttpClientInstrumentation();
                }

                if (opts.UseOtlpExporter)
                {
                    metrics.AddOtlpExporter(o => o.Endpoint = new Uri(opts.OtlpEndpoint));
                }
            });

        return services;
    }
}
