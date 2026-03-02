using System.Security.Claims;
using MarcusPrado.Platform.Abstractions.Context;

namespace MarcusPrado.Platform.TestKit.Fakes;

/// <summary>
/// Configurable <see cref="IUserContext"/> for tests. Build using the static
/// factory methods or assign properties directly.
/// </summary>
public sealed class FakeUserContext : IUserContext
{
    /// <inheritdoc/>
    public string? UserId { get; set; }

    /// <inheritdoc/>
    public bool IsAuthenticated => UserId is not null;

    /// <inheritdoc/>
    public IReadOnlyCollection<string> Permissions { get; set; } = Array.Empty<string>();

    /// <inheritdoc/>
    public IReadOnlyCollection<string> Scopes { get; set; } = Array.Empty<string>();

    private ClaimsPrincipal? _principal;

    /// <summary>Creates an anonymous (unauthenticated) user context.</summary>
    public static FakeUserContext Anonymous() => new();

    /// <summary>Creates an authenticated context for the specified <paramref name="userId"/>.</summary>
    public static FakeUserContext Authenticated(
        string userId,
        IEnumerable<string>? permissions = null,
        IEnumerable<string>? scopes = null)
    {
        return new FakeUserContext
        {
            UserId = userId,
            Permissions = permissions is null ? Array.Empty<string>() : permissions.ToList(),
            Scopes = scopes is null ? Array.Empty<string>() : scopes.ToList(),
        };
    }

    /// <inheritdoc/>
    public string? GetClaim(string claimType)
        => _principal?.FindFirst(claimType)?.Value;

    /// <inheritdoc/>
    public void SetUser(ClaimsPrincipal principal)
    {
        _principal = principal;
        UserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? principal.FindFirst("sub")?.Value;
        Permissions = principal
            .FindAll("permission")
            .Concat(principal.FindAll("permissions"))
            .Select(c => c.Value)
            .Distinct()
            .ToList();
        Scopes = (principal.FindFirst("scope")?.Value ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToList();
    }
}
