using System.Security.Claims;

namespace MarcusPrado.Platform.Abstractions.Context;

/// <summary>
/// Holds identity information for the authenticated principal of the current
/// request. Populated by the authentication middleware after a successful
/// JWT or API-key validation.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the unique identifier of the authenticated user (the <c>sub</c> JWT
    /// claim), or <c>null</c> for anonymous requests.
    /// </summary>
    string? UserId { get; }

    /// <summary>Returns <c>true</c> when a principal has been resolved for the request.</summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets all permission values granted to the current principal.
    /// Populated from <c>permission</c> / <c>permissions</c> JWT claims.
    /// </summary>
    IReadOnlyCollection<string> Permissions { get; }

    /// <summary>
    /// Gets all OAuth2 scope values granted to the current principal.
    /// Populated from the <c>scope</c> JWT claim (space-separated).
    /// </summary>
    IReadOnlyCollection<string> Scopes { get; }

    /// <summary>
    /// Returns the first value of the claim with the given <paramref name="claimType"/>,
    /// or <c>null</c> when the claim is absent.
    /// </summary>
    string? GetClaim(string claimType);

    /// <summary>
    /// Populates the context from an authenticated <see cref="ClaimsPrincipal"/>.
    /// Called once by the authentication handler after successful token validation.
    /// </summary>
    void SetUser(ClaimsPrincipal principal);
}
