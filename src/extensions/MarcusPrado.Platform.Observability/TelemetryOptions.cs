namespace MarcusPrado.Platform.Observability;

public sealed class TelemetryOptions
{
    public string ServiceName { get; set; } = "platform-service";

    public string ServiceVersion { get; set; } = "1.0.0";

    public string OtlpEndpoint { get; set; } = "http://localhost:4317";

    public bool EnableTracing { get; set; } = true;

    public bool EnableMetrics { get; set; } = true;

    public bool EnableLogs { get; set; } = true;

    public bool EnableRuntimeMetrics { get; set; } = true;

    public bool EnableAspNetCoreInstrumentation { get; set; } = true;

    public bool EnableHttpClientInstrumentation { get; set; } = true;
}
