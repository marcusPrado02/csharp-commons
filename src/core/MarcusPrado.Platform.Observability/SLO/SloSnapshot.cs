namespace MarcusPrado.Platform.Observability.SLO;

/// <summary>
/// A point-in-time sample of request counts used to evaluate an SLO.
/// </summary>
/// <param name="TotalRequests">The total number of requests observed in the window.</param>
/// <param name="FailedRequests">The number of requests that resulted in a failure.</param>
public sealed record SloSnapshot(long TotalRequests, long FailedRequests);
