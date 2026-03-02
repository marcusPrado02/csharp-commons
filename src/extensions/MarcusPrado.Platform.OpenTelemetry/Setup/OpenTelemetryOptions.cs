namespace MarcusPrado.Platform.OpenTelemetry.Setup;

/// <summary>Configuration options for the platform OpenTelemetry setup.</summary>
public sealed class OpenTelemetryOptions
{
    /// <summary>Gets or sets the service name reported to the collector.</summary>
    public string ServiceName { get; set; } = "platform-service";

    /// <summary>Gets or sets the service version reported to the collector.</summary>
    public string ServiceVersion { get; set; } = "0.0.0";

    /// <summary>Gets or sets the OTLP collector endpoint.</summary>
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";

    /// <summary>When true, adds trace and metric exporters configured for OTLP.</summary>
    public bool UseOtlpExporter { get; set; } = true;

    /// <summary>When true, instruments ASP.NET Core request pipeline.</summary>
    public bool InstrumentAspNetCore { get; set; } = true;

    /// <summary>When true, instruments outgoing HttpClient calls.</summary>
    public bool InstrumentHttpClient { get; set; } = true;

    /// <summary>Gets or sets additional <see cref="System.Diagnostics.ActivitySource"/> names to capture.</summary>
    public IReadOnlyList<string> AdditionalActivitySources { get; set; } = Array.Empty<string>();
}
