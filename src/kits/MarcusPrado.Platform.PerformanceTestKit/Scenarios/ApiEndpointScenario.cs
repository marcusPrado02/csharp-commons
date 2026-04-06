namespace MarcusPrado.Platform.PerformanceTestKit.Scenarios;

/// <summary>
/// A load-test scenario that fires HTTP GET requests to a target URL and reports
/// P50/P95/P99 latencies along with throughput.
/// </summary>
public sealed class ApiEndpointScenario
{
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly LoadTestConfig _config;

    /// <summary>
    /// Initialises a new <see cref="ApiEndpointScenario"/>.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> used to send requests.</param>
    /// <param name="url">The absolute URL of the endpoint under test.</param>
    /// <param name="config">Load test configuration (VUs, duration, optional warmup).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is <see langword="null"/>.
    /// </exception>
    public ApiEndpointScenario(HttpClient httpClient, string url, LoadTestConfig config)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(config);

        _httpClient = httpClient;
        _url = url;
        _config = config;
    }

    /// <summary>
    /// Executes the scenario and returns aggregated HTTP latency and throughput results.
    /// </summary>
    /// <param name="ct">Optional cancellation token to abort the run early.</param>
    /// <returns>A <see cref="LoadTestResult"/> with P50/P95/P99 latencies and throughput.</returns>
    public Task<LoadTestResult> RunAsync(CancellationToken ct = default) =>
        LoadTestRunner.RunAsync(_config, SendRequestAsync, ct);

    private async Task SendRequestAsync(CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync(_url, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}
