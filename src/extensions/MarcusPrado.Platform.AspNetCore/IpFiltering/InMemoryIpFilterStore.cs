namespace MarcusPrado.Platform.AspNetCore.IpFiltering;

/// <summary>
/// In-memory implementation of <see cref="IIpFilterStore"/> that returns lists
/// configured via <see cref="IpFilterOptions"/>.
/// </summary>
public sealed class InMemoryIpFilterStore : IIpFilterStore
{
    private readonly IpFilterOptions _options;

    /// <summary>Initialises the store with the provided options.</summary>
    public InMemoryIpFilterStore(IpFilterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> GetWhitelistAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<string>>(_options.Whitelist.ToList());

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> GetBlacklistAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<string>>(_options.Blacklist.ToList());
}
