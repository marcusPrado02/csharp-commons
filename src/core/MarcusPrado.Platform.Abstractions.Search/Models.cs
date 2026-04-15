namespace MarcusPrado.Platform.Abstractions.Search;

/// <summary>Encapsulates the parameters for a full-text search request against an index.</summary>
/// <param name="IndexName">The name of the search index to query.</param>
/// <param name="Query">The search query string.</param>
/// <param name="Skip">Number of results to skip for pagination (default 0).</param>
/// <param name="Take">Maximum number of results to return (default 10).</param>
/// <param name="Filters">Optional key-value pairs used to narrow results by field values.</param>
/// <param name="SortField">Optional field name to sort results by.</param>
/// <param name="SortDescending">When <see langword="true"/>, results are sorted in descending order.</param>
public sealed record SearchQuery(
    string IndexName, string Query,
    int Skip = 0,
    int Take = 10,
    IReadOnlyDictionary<string, string>? Filters = null,
    string? SortField = null, bool SortDescending = false);

/// <summary>Represents the paginated results returned by a search operation.</summary>
/// <typeparam name="T">The document type of each search hit.</typeparam>
/// <param name="Hits">The list of matching documents for the current page.</param>
/// <param name="Total">The total number of documents matching the query across all pages.</param>
/// <param name="TookMs">The time in milliseconds the search engine took to execute the query.</param>
public sealed record SearchResult<T>(
    IReadOnlyList<T> Hits,
    long Total,
    double TookMs);
