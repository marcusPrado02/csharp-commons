namespace MarcusPrado.Platform.TestKit.Builders;

/// <summary>
/// Named, reusable test-data scenarios that represent common domain states.
/// Use these as a starting point when a specific scenario is needed in a test.
/// </summary>
public static class TestDataScenarios
{
    private static readonly Random Rng = Random.Shared;

    /// <summary>A valid, active tenant in the <c>free</c> tier.</summary>
    public static TenantScenario FreeTenant =>
        new(
            TenantId: Guid.NewGuid().ToString("N"),
            Name: $"Free Tenant {Rng.Next(100, 999)}",
            Plan: "free",
            IsActive: true
        );

    /// <summary>A premium tenant with all features enabled.</summary>
    public static TenantScenario PremiumTenant =>
        new(
            TenantId: Guid.NewGuid().ToString("N"),
            Name: $"Premium Corp {Rng.Next(100, 999)}",
            Plan: "premium",
            IsActive: true
        );

    /// <summary>A suspended (inactive) tenant.</summary>
    public static TenantScenario SuspendedTenant =>
        new(
            TenantId: Guid.NewGuid().ToString("N"),
            Name: $"Suspended Inc {Rng.Next(100, 999)}",
            Plan: "free",
            IsActive: false
        );

    /// <summary>A valid, active user with a set of standard permissions.</summary>
    public static UserScenario ValidUser =>
        new(
            UserId: Guid.NewGuid().ToString("N"),
            Email: $"user.{Rng.Next(100, 999)}@example.test",
            Permissions: ["read", "write"],
            Scopes: ["api:read", "api:write"],
            IsActive: true
        );

    /// <summary>An anonymous user with no permissions.</summary>
    public static UserScenario AnonymousUser =>
        new(UserId: null, Email: null, Permissions: [], Scopes: [], IsActive: false);

    /// <summary>A subscription that has already expired.</summary>
    public static SubscriptionScenario ExpiredSubscription =>
        new(
            SubscriptionId: Guid.NewGuid(),
            Plan: "premium",
            ExpiresAt: DateTimeOffset.UtcNow.AddDays(-30),
            IsActive: false
        );

    /// <summary>A subscription that is valid and has not expired.</summary>
    public static SubscriptionScenario ActiveSubscription =>
        new(
            SubscriptionId: Guid.NewGuid(),
            Plan: "premium",
            ExpiresAt: DateTimeOffset.UtcNow.AddDays(365),
            IsActive: true
        );
}

/// <summary>Represents a tenant scenario used in tests.</summary>
public sealed record TenantScenario(string TenantId, string Name, string Plan, bool IsActive);

/// <summary>Represents a user scenario used in tests.</summary>
public sealed record UserScenario(
    string? UserId,
    string? Email,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Scopes,
    bool IsActive
);

/// <summary>Represents a subscription scenario used in tests.</summary>
public sealed record SubscriptionScenario(Guid SubscriptionId, string Plan, DateTimeOffset ExpiresAt, bool IsActive);
