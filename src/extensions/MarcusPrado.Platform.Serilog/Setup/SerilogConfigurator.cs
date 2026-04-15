using MarcusPrado.Platform.Serilog.Enrichers;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace MarcusPrado.Platform.Serilog.Setup;

/// <summary>Configures Serilog as the platform-standard structured logger.</summary>
public static class SerilogConfigurator
{
    /// <summary>
    /// Adds Serilog to the <paramref name="builder"/> with platform-standard enrichers,
    /// console output, and request logging.
    /// </summary>
    public static IHostApplicationBuilder AddPlatformSerilog(
        this IHostApplicationBuilder builder,
        Action<SerilogOptions>? configure = null
    )
    {
        var opts = new SerilogOptions();
        configure?.Invoke(opts);

        var minLevel = Enum.TryParse<LogEventLevel>(opts.MinimumLevel, ignoreCase: true, out var lvl)
            ? lvl
            : LogEventLevel.Information;

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(minLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.With(new ContextEnricher(opts.ApplicationName, opts.Environment));

        if (opts.UseJsonOutput)
        {
            loggerConfig.WriteTo.Console(new global::Serilog.Formatting.Json.JsonFormatter());
        }
        else
        {
            loggerConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            );
        }

        global::Serilog.Log.Logger = loggerConfig.CreateLogger();

        builder.Services.AddSerilog();
        return builder;
    }
}
