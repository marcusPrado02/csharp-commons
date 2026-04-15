namespace MarcusPrado.Platform.Observability;

/// <summary>Pre-configured platform metrics for standard microservice observability.</summary>
public sealed class PlatformMeter : IDisposable
{
    public const string MeterName = "MarcusPrado.Platform";

    private readonly Meter _meter;

    public PlatformMeter(string serviceName)
    {
        _meter = new Meter(MeterName, "1.0.0");

        RequestCount = _meter.CreateCounter<long>(
            "platform.requests.total",
            "requests",
            "Total HTTP requests processed"
        );

        RequestDuration = _meter.CreateHistogram<double>(
            "platform.request.duration_ms",
            "ms",
            "HTTP request duration in milliseconds"
        );

        ActiveRequests = _meter.CreateUpDownCounter<int>(
            "platform.requests.active",
            "requests",
            "Currently active requests"
        );

        ErrorCount = _meter.CreateCounter<long>("platform.errors.total", "errors", "Total errors encountered");

        CacheHitCount = _meter.CreateCounter<long>("platform.cache.hits", "hits", "Cache hit count");

        CacheMissCount = _meter.CreateCounter<long>("platform.cache.misses", "misses", "Cache miss count");
    }

    public Counter<long> RequestCount { get; }

    public Histogram<double> RequestDuration { get; }

    public UpDownCounter<int> ActiveRequests { get; }

    public Counter<long> ErrorCount { get; }

    public Counter<long> CacheHitCount { get; }

    public Counter<long> CacheMissCount { get; }

    public void Dispose() => _meter.Dispose();
}
