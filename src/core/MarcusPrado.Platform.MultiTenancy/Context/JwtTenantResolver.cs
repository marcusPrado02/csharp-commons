using Microsoft.IdentityModel.JsonWebTokens;

namespace MarcusPrado.Platform.MultiTenancy.Context;

/// <summary>
/// Resolves the tenant ID from the <c>tid</c> claim inside
/// the Bearer token passed via the <c>Authorization</c> header.
/// </summary>
public sealed class JwtTenantResolver : ITenantResolver
{
    private const string AuthorizationHeader = "authorization";
    private const string TidClaim            = "tid";

    /// <inheritdoc />
    public string? Resolve(IReadOnlyDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        if (!headers.TryGetValue(AuthorizationHeader, out var authValue))
            return null;

        var token = authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authValue["Bearer ".Length..].Trim()
            : null;

        if (string.IsNullOrWhiteSpace(token)) return null;

        var handler = new JsonWebTokenHandler();
        if (!handler.CanReadToken(token)) return null;

        var jwt = handler.ReadJsonWebToken(token);
        return jwt.TryGetClaim(TidClaim, out var claim) ? claim.Value : null;
    }
}
