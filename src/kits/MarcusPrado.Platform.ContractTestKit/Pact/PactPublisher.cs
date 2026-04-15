using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MarcusPrado.Platform.ContractTestKit.Pact;

/// <summary>
/// Publishes a Pact JSON file to a Pact Broker along with Git metadata.
/// </summary>
public sealed class PactPublisher
{
    private readonly HttpClient _httpClient;
    private readonly string _brokerBaseUrl;

    /// <summary>
    /// Initialises a new <see cref="PactPublisher"/> with the given HTTP client and broker URL.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> used to communicate with the broker.</param>
    /// <param name="brokerBaseUrl">The base URL of the Pact Broker (e.g. <c>https://broker.example.com</c>).</param>
    public PactPublisher(HttpClient httpClient, string brokerBaseUrl)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(brokerBaseUrl);
        _httpClient = httpClient;
        _brokerBaseUrl = brokerBaseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Publishes the Pact file at <paramref name="pactFilePath"/> to the Pact Broker with the
    /// supplied Git metadata.
    /// </summary>
    /// <param name="pactFilePath">Path to the Pact JSON file to publish.</param>
    /// <param name="consumerName">The consumer service name as declared in the Pact file.</param>
    /// <param name="version">The semantic version string (e.g. <c>1.2.3</c>).</param>
    /// <param name="branch">The Git branch name (e.g. <c>main</c>).</param>
    /// <param name="commitSha">The full or short Git commit SHA.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that resolves to <see langword="true"/> if the publish succeeded (HTTP 2xx).</returns>
    /// <exception cref="FileNotFoundException">Thrown when the Pact file does not exist.</exception>
    public async Task<bool> PublishAsync(
        string pactFilePath,
        string consumerName,
        string version,
        string branch,
        string commitSha,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pactFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(branch);
        ArgumentException.ThrowIfNullOrWhiteSpace(commitSha);

        if (!File.Exists(pactFilePath))
            throw new FileNotFoundException($"Pact file not found: {pactFilePath}", pactFilePath);

        var pactContent = await File.ReadAllTextAsync(pactFilePath, cancellationToken).ConfigureAwait(false);

        // Pact Broker REST API: PUT /pacts/provider/{provider}/consumer/{consumer}/version/{version}
        // We embed git metadata via the "pacticipantVersionMetadata" property in the request body.
        var payload = new
        {
            pact = pactContent,
            branch,
            version,
            buildUrl = (string?)null,
            tags = Array.Empty<string>(),
            consumerVersionMetadata = new Dictionary<string, string> { ["commitSha"] = commitSha, ["branch"] = branch },
        };

        var url =
            $"{_brokerBaseUrl}/pacts/provider/unknown/consumer/{Uri.EscapeDataString(consumerName)}/version/{Uri.EscapeDataString(version)}";
        var json = JsonSerializer.Serialize(payload);
        using var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PutAsync(url, requestContent, cancellationToken).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }
}
