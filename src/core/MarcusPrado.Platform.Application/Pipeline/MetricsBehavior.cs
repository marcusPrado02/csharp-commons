namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>Emits Prometheus/OTEL metrics (latency, count, error rate) per handler.</summary>
public class MetricsBehavior : IPipelineBehavior { }
