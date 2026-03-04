namespace MarcusPrado.Platform.MultiTenancy.Context;

/// <summary>Resolves the tenant ID from the <c>x-tenant-id</c> request header.</summary>
public sealed class HeaderTenantResolver : ITenantResolver
{
    /// <summary>The header key used to carry the tenant ID.</summary>
    public const string HeaderKey = "x-tenant-id";

    /// <inheritdoc />
    public string? Resolve(IReadOnlyDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);
        if (!headers.TryGetValue(HeaderKey, out var v))
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    }
}
