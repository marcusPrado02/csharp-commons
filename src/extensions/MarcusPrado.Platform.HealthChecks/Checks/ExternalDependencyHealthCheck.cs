using System.Net.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MarcusPrado.Platform.HealthChecks.Checks;

/// <summary>
/// Health check that probes an external HTTP dependency with an aggressive
/// timeout (default: 1 second).
/// Returns <see cref="HealthStatus.Degraded"/> for slow or non-2xx responses
/// and <see cref="HealthStatus.Unhealthy"/> when the endpoint is unreachable.
/// </summary>
public sealed class ExternalDependencyHealthCheck : IHealthCheck
{
    private readonly HttpClient _http;
    private readonly string _url;
    private readonly TimeSpan _timeout;

    /// <summary>
    /// Initialises with an HTTP client, the URL to probe, and an optional timeout.
    /// </summary>
    /// <param name="http">HTTP client to use.</param>
    /// <param name="url">The URL to probe.</param>
    /// <param name="timeout">Probe timeout (default: 1 second).</param>
    public ExternalDependencyHealthCheck(
        HttpClient http,
        string url,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        _http = http;
        _url = url;
        _timeout = timeout ?? TimeSpan.FromSeconds(1);
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        try
        {
            using var response = await _http.GetAsync(_url, cts.Token);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"External dependency '{_url}' is reachable.")
                : HealthCheckResult.Degraded(
                    $"External dependency '{_url}' returned HTTP {(int)response.StatusCode}.");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Degraded(
                $"External dependency '{_url}' timed out after {_timeout.TotalSeconds:F1}s.");
        }
        catch (HttpRequestException ex)
        {
            return HealthCheckResult.Unhealthy(
                $"External dependency '{_url}' is unreachable.",
                ex);
        }
    }
}
