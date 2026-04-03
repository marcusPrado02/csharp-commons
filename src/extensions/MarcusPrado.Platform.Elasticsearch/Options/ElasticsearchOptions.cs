namespace MarcusPrado.Platform.Elasticsearch.Options;

/// <summary>Configuration for the Elasticsearch adapter.</summary>
public sealed class ElasticsearchOptions
{
    /// <summary>Gets or sets the Elasticsearch node URI.</summary>
    public string Url { get; set; } = "http://localhost:9200";

    /// <summary>Gets or sets the optional username for basic authentication.</summary>
    public string? Username { get; set; }

    /// <summary>Gets or sets the optional password for basic authentication.</summary>
    public string? Password { get; set; }

    /// <summary>Gets or sets the optional certificate fingerprint for cloud deployments.</summary>
    public string? CloudId { get; set; }

    /// <summary>Gets or sets the default number of results when <see cref="SearchQuery.Take"/> is not specified.</summary>
    public int DefaultPageSize { get; set; } = 10;
}
