using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace MarcusPrado.Platform.Http.Clients;

/// <summary>
/// Base class for typed HTTP clients with built-in correlation and tenant
/// header propagation (via the registered <see cref="DelegatingHandler"/> chain)
/// and JSON helpers.
/// </summary>
public abstract class TypedHttpClient
{
    /// <summary>The underlying HTTP client.</summary>
    protected HttpClient Http { get; }

    private readonly ILogger _logger;

    private static readonly JsonSerializerOptions _jsonOpts =
        new(JsonSerializerDefaults.Web);

    /// <summary>Initialises with the factory-provided client and a logger.</summary>
    protected TypedHttpClient(HttpClient http, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(logger);
        Http    = http;
        _logger = logger;
    }

    /// <summary>
    /// Sends a GET request and deserialises the JSON response body.
    /// Throws on non-success status codes.
    /// </summary>
    protected async Task<T?> GetAsync<T>(
        string requestUri,
        CancellationToken ct = default)
    {
        TypedHttpClientLog.OutgoingRequest(_logger, "GET", requestUri);
        using var response = await Http.GetAsync(requestUri, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_jsonOpts, ct);
    }

    /// <summary>
    /// Sends a POST request with a JSON body and deserialises the response.
    /// Throws on non-success status codes.
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string requestUri,
        TRequest body,
        CancellationToken ct = default)
    {
        TypedHttpClientLog.OutgoingRequest(_logger, "POST", requestUri);
        using var response = await Http.PostAsJsonAsync(requestUri, body, _jsonOpts, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOpts, ct);
    }
}

internal static partial class TypedHttpClientLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "HTTP {Method} {Uri}")]
    internal static partial void OutgoingRequest(ILogger logger, string method, string uri);
}
