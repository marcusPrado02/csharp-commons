namespace MarcusPrado.Platform.Abstractions.Search;

public sealed record SearchQuery(
    string IndexName, string Query,
    int    Skip       = 0,
    int    Take       = 10,
    IReadOnlyDictionary<string, string>? Filters = null,
    string? SortField = null, bool SortDescending = false);

public sealed record SearchResult<T>(
    IReadOnlyList<T> Hits,
    long             Total,
    double           TookMs);
