using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MarcusPrado.Platform.ContractTestKit.Pact;

/// <summary>
/// Wraps <see cref="WebApplicationFactory{TEntryPoint}"/> to run HTTP contract verification
/// against a Pact JSON file on disk.
/// </summary>
/// <typeparam name="TEntryPoint">The entry point (Program/Startup) of the application under test.</typeparam>
public sealed class PactVerifier<TEntryPoint> : IDisposable
    where TEntryPoint : class
{
    private readonly HttpClient _client;
    private readonly bool _ownsClient;
    private bool _disposed;

    /// <summary>
    /// Initialises a new <see cref="PactVerifier{TEntryPoint}"/> using the provided factory.
    /// The factory creates and owns the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="factory">The <see cref="WebApplicationFactory{TEntryPoint}"/> that creates the test server.</param>
    public PactVerifier(WebApplicationFactory<TEntryPoint> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _client = factory.CreateClient();
        _ownsClient = true;
    }

    /// <summary>
    /// Initialises a new <see cref="PactVerifier{TEntryPoint}"/> using a pre-built
    /// <see cref="HttpClient"/> (e.g. from <c>TestServer.CreateClient()</c>).
    /// Ownership of <paramref name="httpClient"/> is transferred to this instance.
    /// </summary>
    /// <param name="httpClient">An <see cref="HttpClient"/> pointed at the system under test.</param>
    public PactVerifier(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _client = httpClient;
        _ownsClient = true;
    }

    /// <summary>
    /// Reads the Pact JSON file at <paramref name="pactFilePath"/>, replays each interaction against
    /// the test server, and returns a <see cref="ContractVerificationResult"/> for every interaction.
    /// </summary>
    /// <param name="pactFilePath">Absolute or relative path to the Pact JSON file.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>
    /// A list of <see cref="ContractVerificationResult"/> — one per interaction in the Pact file.
    /// </returns>
    /// <exception cref="FileNotFoundException">Thrown when <paramref name="pactFilePath"/> does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the Pact JSON does not contain an <c>interactions</c> array.</exception>
    public async Task<IReadOnlyList<ContractVerificationResult>> VerifyAsync(
        string pactFilePath,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pactFilePath);

        if (!File.Exists(pactFilePath))
            throw new FileNotFoundException($"Pact file not found: {pactFilePath}", pactFilePath);

        var json = await File.ReadAllTextAsync(pactFilePath, cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (
            !root.TryGetProperty("interactions", out var interactionsElement)
            || interactionsElement.ValueKind != JsonValueKind.Array
        )
        {
            throw new InvalidOperationException("The Pact file does not contain a valid 'interactions' array.");
        }

        var results = new List<ContractVerificationResult>();

        foreach (var interaction in interactionsElement.EnumerateArray())
        {
            var description = interaction.TryGetProperty("description", out var desc)
                ? desc.GetString() ?? "(no description)"
                : "(no description)";

            try
            {
                var result = await VerifyInteractionAsync(interaction, description, cancellationToken)
                    .ConfigureAwait(false);
                results.Add(result);
            }
            catch (HttpRequestException ex)
            {
                results.Add(new ContractVerificationResult(description, false, ex.Message));
            }
            catch (TaskCanceledException ex)
            {
                results.Add(new ContractVerificationResult(description, false, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                results.Add(new ContractVerificationResult(description, false, ex.Message));
            }
        }

        return results;
    }

    private async Task<ContractVerificationResult> VerifyInteractionAsync(
        JsonElement interaction,
        string description,
        CancellationToken cancellationToken
    )
    {
        if (!interaction.TryGetProperty("request", out var request))
            return new ContractVerificationResult(description, false, "Interaction has no 'request' element.");

        var method = request.TryGetProperty("method", out var m) ? m.GetString()?.ToUpperInvariant() ?? "GET" : "GET";
        var path = request.TryGetProperty("path", out var p) ? p.GetString() ?? "/" : "/";

        HttpContent? content = null;
        if (request.TryGetProperty("body", out var bodyElement))
        {
            var bodyJson = bodyElement.GetRawText();
            content = new StringContent(bodyJson, System.Text.Encoding.UTF8, "application/json");
        }

        var httpRequest = new HttpRequestMessage(new HttpMethod(method), path) { Content = content };

        // Forward headers defined in the pact
        if (request.TryGetProperty("headers", out var headersElement))
        {
            foreach (var header in headersElement.EnumerateObject())
            {
                var value = header.Value.GetString();
                if (value is not null)
                    httpRequest.Headers.TryAddWithoutValidation(header.Name, value);
            }
        }

        using var response = await _client.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

        if (!interaction.TryGetProperty("response", out var expectedResponse))
            return new ContractVerificationResult(description, true, null);

        // Verify status code
        if (expectedResponse.TryGetProperty("status", out var statusElement))
        {
            var expectedStatus = statusElement.GetInt32();
            if ((int)response.StatusCode != expectedStatus)
            {
                return new ContractVerificationResult(
                    description,
                    false,
                    $"Expected status {expectedStatus} but got {(int)response.StatusCode}."
                );
            }
        }

        // Verify body if present
        if (expectedResponse.TryGetProperty("body", out var expectedBody))
        {
            var actualBodyJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                using var actualDoc = JsonDocument.Parse(actualBodyJson);
                var expectedBodyJson = expectedBody.GetRawText();
                using var expectedDoc = JsonDocument.Parse(expectedBodyJson);

                if (!JsonElementEquals(actualDoc.RootElement, expectedDoc.RootElement))
                {
                    return new ContractVerificationResult(
                        description,
                        false,
                        $"Response body mismatch. Expected: {expectedBodyJson}. Actual: {actualBodyJson}."
                    );
                }
            }
            catch (JsonException ex)
            {
                return new ContractVerificationResult(description, false, $"JSON parse error: {ex.Message}");
            }
        }

        return new ContractVerificationResult(description, true, null);
    }

    private static bool JsonElementEquals(JsonElement a, JsonElement b)
    {
        if (a.ValueKind != b.ValueKind)
            return false;

        return a.ValueKind switch
        {
            JsonValueKind.Object => JsonObjectEquals(a, b),
            JsonValueKind.Array => JsonArrayEquals(a, b),
            JsonValueKind.String => a.GetString() == b.GetString(),
            JsonValueKind.Number => a.GetRawText() == b.GetRawText(),
            JsonValueKind.True or JsonValueKind.False => a.GetBoolean() == b.GetBoolean(),
            JsonValueKind.Null => true,
            _ => a.GetRawText() == b.GetRawText(),
        };
    }

    private static bool JsonObjectEquals(JsonElement a, JsonElement b)
    {
        var aProps = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var prop in a.EnumerateObject())
            aProps[prop.Name] = prop.Value;

        foreach (var prop in b.EnumerateObject())
        {
            if (!aProps.TryGetValue(prop.Name, out var aVal))
                return false;
            if (!JsonElementEquals(aVal, prop.Value))
                return false;
        }

        return true;
    }

    private static bool JsonArrayEquals(JsonElement a, JsonElement b)
    {
        var aArr = a.EnumerateArray().ToArray();
        var bArr = b.EnumerateArray().ToArray();

        if (aArr.Length != bArr.Length)
            return false;

        for (var i = 0; i < aArr.Length; i++)
        {
            if (!JsonElementEquals(aArr[i], bArr[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        if (_ownsClient)
            _client.Dispose();
    }
}
