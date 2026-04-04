namespace MarcusPrado.Platform.AspNetCore.IpFiltering;

public interface IIpFilterStore
{
    Task<IReadOnlyList<string>> GetWhitelistAsync(CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetBlacklistAsync(CancellationToken ct = default);
}
