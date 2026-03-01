namespace MarcusPrado.Platform.Abstractions.Errors;

/// <summary>
/// Categorizes an <see cref="Error"/> by its origin and semantic meaning,
/// enabling callers to map failures to appropriate HTTP status codes,
/// retry strategies and user-facing messages without inspecting error codes.
/// </summary>
/// <remarks>
/// Recommended HTTP mapping:
/// <list type="table">
///   <item><term><see cref="Validation"/></term><description>422 Unprocessable Entity (or 400)</description></item>
///   <item><term><see cref="NotFound"/></term><description>404 Not Found</description></item>
///   <item><term><see cref="Conflict"/></term><description>409 Conflict</description></item>
///   <item><term><see cref="Unauthorized"/></term><description>401 Unauthorized</description></item>
///   <item><term><see cref="Forbidden"/></term><description>403 Forbidden</description></item>
///   <item><term><see cref="Technical"/></term><description>500 Internal Server Error</description></item>
///   <item><term><see cref="External"/></term><description>502 Bad Gateway</description></item>
///   <item><term><see cref="Timeout"/></term><description>504 Gateway Timeout</description></item>
///   <item><term><see cref="Unavailable"/></term><description>503 Service Unavailable</description></item>
/// </list>
/// </remarks>
public enum ErrorCategory
{
    /// <summary>Input failed business or schema validation rules.</summary>
    Validation,

    /// <summary>The requested resource or aggregate was not found.</summary>
    NotFound,

    /// <summary>The operation conflicts with existing state (e.g. duplicate key, stale version).</summary>
    Conflict,

    /// <summary>The caller is not authenticated — identity could not be established.</summary>
    Unauthorized,

    /// <summary>The caller is authenticated but lacks permission for this resource or action.</summary>
    Forbidden,

    /// <summary>An unexpected internal error occurred that is not the caller's fault.</summary>
    Technical,

    /// <summary>A downstream external dependency returned an error or unexpected response.</summary>
    External,

    /// <summary>The operation exceeded its allotted time budget.</summary>
    Timeout,

    /// <summary>The service or a critical dependency is temporarily unavailable.</summary>
    Unavailable,
}
