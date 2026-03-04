namespace MarcusPrado.Platform.MultiTenancy.Context;

/// <summary>Resolves the current tenant ID from a given context.</summary>
public interface ITenantResolver
{
    /// <summary>
    /// Attempts to resolve a tenant ID from <paramref name="headers"/>.
    /// Returns <see langword="null"/> when the tenant cannot be resolved.
    /// </summary>
    string? Resolve(IReadOnlyDictionary<string, string> headers);
}
