using System.Text;

namespace MarcusPrado.Platform.PerformanceTestKit;

/// <summary>
/// Aggregated result of a load test run containing throughput and latency statistics.
/// </summary>
public sealed record LoadTestResult
{
    /// <summary>Gets the total number of requests that were executed.</summary>
    public long TotalRequests { get; init; }

    /// <summary>Gets the number of requests that resulted in an error.</summary>
    public long ErrorCount { get; init; }

    /// <summary>Gets the measured throughput in requests per second.</summary>
    public double ThroughputRps { get; init; }

    /// <summary>Gets the 50th-percentile (median) response time in milliseconds.</summary>
    public double P50Ms { get; init; }

    /// <summary>Gets the 95th-percentile response time in milliseconds.</summary>
    public double P95Ms { get; init; }

    /// <summary>Gets the 99th-percentile response time in milliseconds.</summary>
    public double P99Ms { get; init; }

    /// <summary>
    /// Gets the ratio of failed requests to total requests.
    /// Returns <c>0</c> when <see cref="TotalRequests"/> is zero.
    /// </summary>
    public double ErrorRate => TotalRequests > 0 ? (double)ErrorCount / TotalRequests : 0;

    /// <summary>
    /// Generates a human-readable plain-text report summarising the load test results.
    /// </summary>
    /// <returns>A formatted string containing all key metrics.</returns>
    public string ToReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Load Test Report ===");
        sb.AppendLine($"Total Requests : {TotalRequests}");
        sb.AppendLine($"Errors         : {ErrorCount} ({ErrorRate:P2})");
        sb.AppendLine($"Throughput     : {ThroughputRps:F2} req/s");
        sb.AppendLine($"P50 Latency    : {P50Ms:F2} ms");
        sb.AppendLine($"P95 Latency    : {P95Ms:F2} ms");
        sb.AppendLine($"P99 Latency    : {P99Ms:F2} ms");
        sb.AppendLine("========================");
        return sb.ToString();
    }
}
