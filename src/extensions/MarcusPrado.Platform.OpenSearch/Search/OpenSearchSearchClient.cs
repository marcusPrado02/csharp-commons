using System.Reflection;
using MarcusPrado.Platform.Abstractions.Search;
using OpenSearch.Client;
using AbsSearch = MarcusPrado.Platform.Abstractions.Search;

namespace MarcusPrado.Platform.OpenSearch.Search;

/// <summary>
/// Implements both <see cref="ISearchClient"/> and <see cref="IIndexManager"/>
/// using the OpenSearch.Client driver.
/// </summary>
/// <remarks>
/// OpenSearch.Client requires <c>T : class</c> on generic search/get/index methods.
/// The unconstrained interface methods dispatch to private constrained helpers at runtime.
/// This is safe because OpenSearch documents are always reference types in practice.
/// </remarks>
public sealed class OpenSearchSearchClient : ISearchClient, IIndexManager
{
    private readonly IOpenSearchClient _client;

#pragma warning disable S3011 // Reflection access to private members is intentional here
    private static readonly MethodInfo _searchCoreMethod = typeof(OpenSearchSearchClient).GetMethod(
        nameof(SearchCoreAsync),
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;

    private static readonly MethodInfo _getByIdCoreMethod = typeof(OpenSearchSearchClient).GetMethod(
        nameof(GetByIdCoreAsync),
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;

    private static readonly MethodInfo _indexDocumentCoreMethod = typeof(OpenSearchSearchClient).GetMethod(
        nameof(IndexDocumentCoreAsync),
        BindingFlags.NonPublic | BindingFlags.Instance
    )!;
#pragma warning restore S3011

    /// <summary>Initializes a new instance of <see cref="OpenSearchSearchClient"/>.</summary>
    public OpenSearchSearchClient(IOpenSearchClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    // ── ISearchClient ──────────────────────────────────────────────────────

    /// <inheritdoc />
    public Task<SearchResult<T>> SearchAsync<T>(AbsSearch.SearchQuery query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return (Task<SearchResult<T>>)_searchCoreMethod.MakeGenericMethod(typeof(T)).Invoke(this, [query, ct])!;
    }

    /// <inheritdoc />
    public Task<T?> GetByIdAsync<T>(string indexName, string id, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return (Task<T?>)_getByIdCoreMethod.MakeGenericMethod(typeof(T)).Invoke(this, [indexName, id, ct])!;
    }

    // ── IIndexManager ──────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task CreateIndexAsync(string indexName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        var response = await _client.Indices.CreateAsync(new CreateIndexRequest(indexName), ct).ConfigureAwait(false);

        if (!response.IsValid)
            throw new InvalidOperationException(response.DebugInformation);
    }

    /// <inheritdoc />
    public async Task DeleteIndexAsync(string indexName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        var response = await _client.Indices.DeleteAsync(new DeleteIndexRequest(indexName), ct).ConfigureAwait(false);

        if (!response.IsValid)
            throw new InvalidOperationException(response.DebugInformation);
    }

    /// <inheritdoc />
    public async Task<bool> IndexExistsAsync(string indexName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        var response = await _client.Indices.ExistsAsync(new IndexExistsRequest(indexName), ct).ConfigureAwait(false);

        return response.Exists;
    }

    /// <inheritdoc />
    public Task IndexDocumentAsync<T>(string indexName, string id, T document, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(document);
        return (Task)_indexDocumentCoreMethod.MakeGenericMethod(typeof(T)).Invoke(this, [indexName, id, document, ct])!;
    }

    /// <inheritdoc />
    public async Task DeleteDocumentAsync(string indexName, string id, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var response = await _client.DeleteAsync(new DeleteRequest(indexName, id), ct).ConfigureAwait(false);

        if (!response.IsValid)
            throw new InvalidOperationException(response.DebugInformation);
    }

    // ── Private constrained helpers ────────────────────────────────────────

    private async Task<SearchResult<T>> SearchCoreAsync<T>(AbsSearch.SearchQuery query, CancellationToken ct)
        where T : class
    {
        var response = await _client
            .SearchAsync<T>(
                s =>
                {
                    s = s.Index(query.IndexName).From(query.Skip).Size(query.Take);

                    if (query.Filters is { Count: > 0 })
                    {
                        s = s.Query(q =>
                            q.Bool(b =>
                                b.Must(m => m.QueryString(qs => qs.Query(query.Query)))
                                    .Filter(BuildFilterClauses<T>(query.Filters))
                            )
                        );
                    }
                    else
                    {
                        s = s.Query(q => q.QueryString(qs => qs.Query(query.Query)));
                    }

                    if (query.SortField is not null)
                    {
                        s = s.Sort(so =>
                            query.SortDescending ? so.Descending(query.SortField) : so.Ascending(query.SortField)
                        );
                    }

                    return s;
                },
                ct
            )
            .ConfigureAwait(false);

        if (!response.IsValid)
            throw new InvalidOperationException(response.DebugInformation);

        return new SearchResult<T>(response.Documents.ToList(), response.Total, response.Took);
    }

    private async Task<T?> GetByIdCoreAsync<T>(string indexName, string id, CancellationToken ct)
        where T : class
    {
        var response = await _client.GetAsync<T>(new GetRequest(indexName, id), ct).ConfigureAwait(false);

        return response.Found ? response.Source : default;
    }

    private async Task IndexDocumentCoreAsync<T>(string indexName, string id, T document, CancellationToken ct)
        where T : class
    {
        var response = await _client.IndexAsync(document, i => i.Index(indexName).Id(id), ct).ConfigureAwait(false);

        if (!response.IsValid)
            throw new InvalidOperationException(response.DebugInformation);
    }

    private static Func<QueryContainerDescriptor<T>, QueryContainer>[] BuildFilterClauses<T>(
        IReadOnlyDictionary<string, string> filters
    )
        where T : class
    {
        var clauses = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>(filters.Count);
        foreach (var (field, value) in filters)
        {
            var f = field;
            var v = value;
            clauses.Add(q => q.Term(t => t.Field(f).Value(v)));
        }

        return clauses.ToArray();
    }
}
