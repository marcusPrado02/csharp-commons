namespace MarcusPrado.Platform.AspNetCore.Cors;

/// <summary>
/// Predefined CORS profiles for different deployment environments.
/// </summary>
public enum PlatformCorsProfile
{
    /// <summary>Allows any origin, method, and header. For local development only.</summary>
    DevPermissive,

    /// <summary>Restricts to a configured set of origins with permissive methods/headers.</summary>
    StagingRestricted,

    /// <summary>Restricts to a configured set of origins with strict methods/headers and preflight cache.</summary>
    ProductionLocked
}
