using MarcusPrado.Platform.AspNetCore.Auth.Requirements;

namespace MarcusPrado.Platform.AspNetCore.Auth.Handlers;

/// <summary>
/// Handles <see cref="ScopeRequirement"/> by verifying that the current
/// principal holds the required OAuth2 scope.  Supports both a single
/// space-separated <c>scope</c> claim value and individual <c>scp</c> claims.
/// </summary>
public sealed class ScopeAuthorizationHandler : AuthorizationHandler<ScopeRequirement>
{
    /// <inheritdoc />
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
    {
        var required = requirement.Scope.Value;

        // Try individual "scp" claims first
        if (context.User.HasClaim("scp", required))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Fall back to space-separated "scope" claim (RFC 8693)
        var scopeClaim = context.User.FindFirstValue("scope") ?? context.User.FindFirstValue("scp");

        if (scopeClaim is not null)
        {
            var scopes = scopeClaim.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (scopes.Contains(required, StringComparer.OrdinalIgnoreCase))
                context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
