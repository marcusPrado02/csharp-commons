namespace MarcusPrado.Platform.Serilog.Setup;

/// <summary>Configuration options for the platform Serilog setup.</summary>
public sealed class SerilogOptions
{
    /// <summary>Gets or sets the application name enriched into every log event.</summary>
    public string ApplicationName { get; set; } = "platform-service";

    /// <summary>Gets or sets the deployment environment (development, staging, production).</summary>
    public string Environment { get; set; } = "development";

    /// <summary>When true, emits colored console output (development mode).</summary>
    public bool UseColoredConsole { get; set; } = true;

    /// <summary>When true, emits compact JSON output (production mode).</summary>
    public bool UseJsonOutput { get; set; }

    /// <summary>Gets or sets the minimum log level.</summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>Gets or sets request paths excluded from request logging.</summary>
    public IReadOnlyList<string> ExcludedPaths { get; set; } =
        new[] { "/health", "/health/live", "/health/ready", "/health/detail", "/ping", "/metrics" };
}
