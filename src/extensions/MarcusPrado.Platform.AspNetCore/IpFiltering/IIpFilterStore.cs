namespace MarcusPrado.Platform.AspNetCore.IpFiltering;

/// <summary>Provides access to the IP allow-list and deny-list used by the IP filtering middleware.</summary>
public interface IIpFilterStore
{
    /// <summary>Returns the current list of allowed IP addresses or CIDR ranges.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of whitelisted IP/CIDR entries.</returns>
    Task<IReadOnlyList<string>> GetWhitelistAsync(CancellationToken ct = default);

    /// <summary>Returns the current list of blocked IP addresses or CIDR ranges.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of blacklisted IP/CIDR entries.</returns>
    Task<IReadOnlyList<string>> GetBlacklistAsync(CancellationToken ct = default);
}
