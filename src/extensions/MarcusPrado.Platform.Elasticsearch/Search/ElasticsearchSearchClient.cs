using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using MarcusPrado.Platform.Abstractions.Search;
using MarcusPrado.Platform.Elasticsearch.Options;
using AbsSearch = MarcusPrado.Platform.Abstractions.Search;

namespace MarcusPrado.Platform.Elasticsearch.Search;

/// <summary>
/// Implements both <see cref="ISearchClient"/> and <see cref="IIndexManager"/>
/// using the Elastic.Clients.Elasticsearch 8.x driver.
/// </summary>
public sealed class ElasticsearchSearchClient : ISearchClient, IIndexManager
{
    private readonly ElasticsearchClient _client;

    /// <summary>Initializes a new instance of <see cref="ElasticsearchSearchClient"/>.</summary>
    public ElasticsearchSearchClient(ElasticsearchClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <inheritdoc />
    public async Task<SearchResult<T>> SearchAsync<T>(
        AbsSearch.SearchQuery query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var filterClauses = BuildFilterClauses(query.Filters);

        var response = await _client.SearchAsync<T>(s =>
        {
            s.Index(query.IndexName)
             .From(query.Skip)
             .Size(query.Take);

            if (filterClauses.Count > 0)
            {
                s.Query(q => q.Bool(b => b
                    .Must(m => m.QueryString(qs => qs.Query(query.Query)))
                    .Filter(filterClauses)));
            }
            else
            {
                s.Query(q => q.QueryString(qs => qs.Query(query.Query)));
            }

            if (query.SortField is not null)
            {
                s.Sort(so => so.Field(
                    query.SortField!,
                    f => { f.Order(query.SortDescending ? SortOrder.Desc : SortOrder.Asc); }));
            }
        }, ct).ConfigureAwait(false);

        if (!response.IsValidResponse)
            throw new InvalidOperationException(response.DebugInformation);

        return new SearchResult<T>(
            response.Documents.ToList(),
            response.Total,
            response.Took);
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync<T>(
        string indexName, string id, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var response = await _client.GetAsync<T>(
            id, g => g.Index(indexName), ct).ConfigureAwait(false);

        return response.Found ? response.Source : default;
    }

    /// <inheritdoc />
    public async Task CreateIndexAsync(string indexName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        var response = await _client.Indices.CreateAsync(indexName, ct)
            .ConfigureAwait(false);

        if (!response.IsValidResponse)
            throw new InvalidOperationException(response.DebugInformation);
    }

    /// <inheritdoc />
    public async Task DeleteIndexAsync(string indexName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        var response = await _client.Indices.DeleteAsync(indexName, ct)
            .ConfigureAwait(false);

        if (!response.IsValidResponse)
            throw new InvalidOperationException(response.DebugInformation);
    }

    /// <inheritdoc />
    public async Task<bool> IndexExistsAsync(string indexName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        var response = await _client.Indices.ExistsAsync(indexName, ct)
            .ConfigureAwait(false);

        return response.Exists;
    }

    /// <inheritdoc />
    public async Task IndexDocumentAsync<T>(
        string indexName, string id, T document, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(document);

        var response = await _client.IndexAsync(
            document, i => i.Index(indexName).Id(id), ct).ConfigureAwait(false);

        if (!response.IsValidResponse)
            throw new InvalidOperationException(response.DebugInformation);
    }

    /// <inheritdoc />
    public async Task DeleteDocumentAsync(
        string indexName, string id, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var response = await _client.DeleteAsync<object>(
            id, d => d.Index(indexName), ct).ConfigureAwait(false);

        if (!response.IsValidResponse)
            throw new InvalidOperationException(response.DebugInformation);
    }

    private static List<Query> BuildFilterClauses(
        IReadOnlyDictionary<string, string>? filters)
    {
        if (filters is null || filters.Count == 0)
            return [];

        var clauses = new List<Query>(filters.Count);
        foreach (var (field, value) in filters)
        {
            clauses.Add(new TermQuery(field!) { Value = value });
        }

        return clauses;
    }
}
