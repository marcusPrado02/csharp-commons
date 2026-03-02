using MarcusPrado.Platform.Security.Authorization;

namespace MarcusPrado.Platform.AspNetCore.Auth.Requirements;

/// <summary>
/// ASP.NET Core authorization requirement that mandates a specific OAuth2
/// <see cref="Scope"/> be present in the principal's <c>scope</c> claim.
/// </summary>
/// <param name="Scope">The scope that must be present.</param>
public sealed record ScopeRequirement(Scope Scope) : IAuthorizationRequirement;
