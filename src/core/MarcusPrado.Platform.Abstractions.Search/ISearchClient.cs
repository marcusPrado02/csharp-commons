namespace MarcusPrado.Platform.Abstractions.Search;

/// <summary>Executes full-text search queries against an index.</summary>
public interface ISearchClient
{
    /// <summary>Performs a full-text search and returns typed results.</summary>
    Task<SearchResult<T>> SearchAsync<T>(SearchQuery query, CancellationToken ct = default);

    /// <summary>Retrieves a single document by its index ID.</summary>
    Task<T?> GetByIdAsync<T>(string indexName, string id, CancellationToken ct = default);
}

/// <summary>Manages search indexes (create, delete, index documents).</summary>
public interface IIndexManager
{
    /// <summary>Creates a new index with default settings.</summary>
    Task CreateIndexAsync(string indexName, CancellationToken ct = default);

    /// <summary>Deletes the given index and all its documents.</summary>
    Task DeleteIndexAsync(string indexName, CancellationToken ct = default);

    /// <summary>Returns <see langword="true"/> if the index exists.</summary>
    Task<bool> IndexExistsAsync(string indexName, CancellationToken ct = default);

    /// <summary>Indexes (upserts) a single document.</summary>
    Task IndexDocumentAsync<T>(string indexName, string id, T document, CancellationToken ct = default);

    /// <summary>Deletes a single document from the index.</summary>
    Task DeleteDocumentAsync(string indexName, string id, CancellationToken ct = default);
}
