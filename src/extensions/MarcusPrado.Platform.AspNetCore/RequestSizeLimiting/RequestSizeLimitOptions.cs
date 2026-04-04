namespace MarcusPrado.Platform.AspNetCore.RequestSizeLimiting;

/// <summary>
/// Configuration options for the request size limit middleware.
/// </summary>
public sealed class RequestSizeLimitOptions
{
    private const long OneMegabyte     = 1L * 1024 * 1024;
    private const long TenMegabytes    = 10L * 1024 * 1024;
    private const long HundredMegabytes = 100L * 1024 * 1024;

    /// <summary>
    /// Per-tier byte limits. Defaults:
    /// <list type="bullet">
    ///   <item><see cref="RequestSizeTier.Free"/> → 1 MB (1 048 576 bytes)</item>
    ///   <item><see cref="RequestSizeTier.Pro"/> → 10 MB</item>
    ///   <item><see cref="RequestSizeTier.Enterprise"/> → 100 MB</item>
    /// </list>
    /// </summary>
    public Dictionary<RequestSizeTier, long> TierLimits { get; set; } = new()
    {
        [RequestSizeTier.Free]       = OneMegabyte,
        [RequestSizeTier.Pro]        = TenMegabytes,
        [RequestSizeTier.Enterprise] = HundredMegabytes
    };

    /// <summary>
    /// Tier applied when <see cref="TierResolver"/> returns no result or is not set.
    /// Defaults to <see cref="RequestSizeTier.Free"/>.
    /// </summary>
    public RequestSizeTier DefaultTier { get; set; } = RequestSizeTier.Free;

    /// <summary>
    /// Delegate invoked per request to determine which tier applies.
    /// Return <c>null</c> to fall back to <see cref="DefaultTier"/>.
    /// Default implementation always returns <see cref="DefaultTier"/>.
    /// </summary>
    public Func<HttpContext, RequestSizeTier> TierResolver { get; set; } = _ => RequestSizeTier.Free;

    /// <summary>
    /// Returns the byte limit for the given tier, falling back to the <see cref="RequestSizeTier.Free"/>
    /// limit if the tier is not present in <see cref="TierLimits"/>.
    /// </summary>
    internal long GetLimit(RequestSizeTier tier)
        => TierLimits.TryGetValue(tier, out var limit) ? limit : OneMegabyte;
}
