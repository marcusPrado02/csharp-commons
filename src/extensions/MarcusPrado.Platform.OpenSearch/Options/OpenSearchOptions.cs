namespace MarcusPrado.Platform.OpenSearch.Options;

/// <summary>Configuration for the OpenSearch adapter.</summary>
public sealed class OpenSearchOptions
{
    /// <summary>Gets or sets the OpenSearch node URL.</summary>
    public string Url { get; set; } = "http://localhost:9200";

    /// <summary>Gets or sets the optional basic-auth username.</summary>
    public string? Username { get; set; }

    /// <summary>Gets or sets the optional basic-auth password.</summary>
    public string? Password { get; set; }

    /// <summary>Gets or sets the default page size used when <see cref="MarcusPrado.Platform.Abstractions.Search.SearchQuery.Take"/> is not specified.</summary>
    public int DefaultPageSize { get; set; } = 10;
}
