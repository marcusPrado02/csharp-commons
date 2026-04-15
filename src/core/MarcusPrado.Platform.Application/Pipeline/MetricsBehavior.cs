using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>
/// Records per-request metrics: duration histogram and success/failure counters.
/// Registered as the second behavior (order 2) so timings include validation and handlers.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly Meter _appMeter = new("MarcusPrado.Platform", "1.0.0");

    private static readonly Histogram<long> _durationHistogram =
        _appMeter.CreateHistogram<long>("command.duration_ms", "ms", "Time to handle a request.");

    private static readonly Counter<long> _successCounter =
        _appMeter.CreateCounter<long>("command.success", "requests", "Total successful requests.");

    private static readonly Counter<long> _failureCounter =
        _appMeter.CreateCounter<long>("command.failure", "requests", "Total failed requests.");

    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;
        var tags = new TagList { { "request_type", requestName } };
        var started = System.Diagnostics.Stopwatch.GetTimestamp();

        try
        {
            var response = await next(cancellationToken);
            var elapsed = (long)System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds;

            _durationHistogram.Record(elapsed, tags);
            _successCounter.Add(1, tags);

            return response;
        }
        catch
        {
            var elapsed = (long)System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds;

            _durationHistogram.Record(elapsed, tags);
            _failureCounter.Add(1, tags);

            throw;
        }
    }
}
