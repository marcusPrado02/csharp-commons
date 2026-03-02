using MarcusPrado.Platform.Abstractions.Context;

namespace MarcusPrado.Platform.AspNetCore.Auth.Internal;

/// <summary>
/// Default <see cref="IUserContext"/> implementation. Registered as
/// <c>Scoped</c> and populated once per request by the authentication handler.
/// </summary>
internal sealed class DefaultUserContext : IUserContext
{
    // Claim types used to extract permissions and scopes from the principal
    private static readonly string[] _permissionClaimTypes =
        ["permission", "permissions", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

    private ClaimsPrincipal? _principal;

    /// <inheritdoc />
    public string? UserId { get; private set; }

    /// <inheritdoc />
    public bool IsAuthenticated { get; private set; }

    /// <inheritdoc />
    public IReadOnlyCollection<string> Permissions { get; private set; }
        = Array.Empty<string>();

    /// <inheritdoc />
    public IReadOnlyCollection<string> Scopes { get; private set; }
        = Array.Empty<string>();

    /// <inheritdoc />
    public string? GetClaim(string claimType)
        => _principal?.FindFirstValue(claimType);

    /// <inheritdoc />
    public void SetUser(ClaimsPrincipal principal)
    {
        _principal      = principal;
        IsAuthenticated = principal.Identity?.IsAuthenticated == true;
        UserId          = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? principal.FindFirstValue("sub");

        // Permissions: collected from multiple claim types
        Permissions = _permissionClaimTypes
            .SelectMany(t => principal.FindAll(t))
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        // Scopes: the "scope" claim is a single space-separated value per RFC 8693
        var scopeClaim = principal.FindFirstValue("scope")
                         ?? principal.FindFirstValue("scp")
                         ?? string.Empty;

        Scopes = scopeClaim
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
