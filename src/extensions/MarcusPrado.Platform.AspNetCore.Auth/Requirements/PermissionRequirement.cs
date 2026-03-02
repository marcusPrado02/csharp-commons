using MarcusPrado.Platform.Security.Authorization;

namespace MarcusPrado.Platform.AspNetCore.Auth.Requirements;

/// <summary>
/// ASP.NET Core authorization requirement that mandates a specific
/// <see cref="Permission"/> be present in the principal's claims.
/// </summary>
/// <param name="Permission">The permission that must be granted.</param>
public sealed record PermissionRequirement(Permission Permission) : IAuthorizationRequirement;
