namespace MarcusPrado.Platform.Observability;

public sealed class OtelHealthCheckPublisher : IHealthCheckPublisher, IDisposable
{
    private readonly Meter _meter;

    private HealthReport? _lastReport;

    public OtelHealthCheckPublisher()
    {
        _meter = new Meter("MarcusPrado.Platform.HealthChecks", "1.0.0");
        _meter.CreateObservableGauge(
            "platform.health.status",
            () => (int)(_lastReport?.Status ?? HealthStatus.Healthy),
            "status",
            "Health check status: 2=Healthy, 1=Degraded, 0=Unhealthy");
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        _lastReport = report;
        return Task.CompletedTask;
    }

    public void Dispose() => _meter.Dispose();
}
