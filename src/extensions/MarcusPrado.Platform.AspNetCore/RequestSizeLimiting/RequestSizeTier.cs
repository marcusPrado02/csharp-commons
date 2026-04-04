namespace MarcusPrado.Platform.AspNetCore.RequestSizeLimiting;

/// <summary>
/// Represents the tenant subscription tier used to determine request size limits.
/// </summary>
public enum RequestSizeTier
{
    /// <summary>Free tier — 1 MB limit.</summary>
    Free,

    /// <summary>Pro tier — 10 MB limit.</summary>
    Pro,

    /// <summary>Enterprise tier — 100 MB limit.</summary>
    Enterprise
}
