using MarcusPrado.Platform.AspNetCore.Auth.Requirements;

namespace MarcusPrado.Platform.AspNetCore.Auth.Handlers;

/// <summary>
/// Handles <see cref="PermissionRequirement"/> by verifying that the current
/// principal holds the required permission in a <c>permission</c> claim.
/// </summary>
public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private static readonly string[] _permissionClaimTypes =
        ["permission", "permissions"];

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var required = requirement.Permission.Value;

        var hasPermission = _permissionClaimTypes
            .Any(t => context.User.HasClaim(t, required));

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
