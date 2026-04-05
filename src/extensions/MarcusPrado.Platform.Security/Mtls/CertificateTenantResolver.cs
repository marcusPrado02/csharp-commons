using System.Security.Cryptography.X509Certificates;

namespace MarcusPrado.Platform.Security.Mtls;

public sealed class CertificateTenantResolver
{
    private readonly MtlsOptions _options;

    public CertificateTenantResolver(MtlsOptions options)
        => _options = options;

    /// <summary>
    /// Attempts to extract a tenant ID from the certificate.
    /// First tries the custom SAN OID, then falls back to the CN field.
    /// </summary>
    public string? ResolveTenantId(X509Certificate2 certificate)
    {
        ArgumentNullException.ThrowIfNull(certificate);

        // 1. Try custom SAN OID
        foreach (var ext in certificate.Extensions)
        {
            if (ext.Oid?.Value == _options.TenantIdSanOid)
                return System.Text.Encoding.UTF8.GetString(ext.RawData).TrimStart('\x04').Trim('\0');
        }

        // 2. Fall back to CN with prefix
        var cn = GetCommonName(certificate.Subject);
        if (cn is not null && cn.StartsWith(_options.TenantCnPrefix, StringComparison.OrdinalIgnoreCase))
            return cn[_options.TenantCnPrefix.Length..];

        return null;
    }

    private static string? GetCommonName(string subject)
    {
        foreach (var part in subject.Split(','))
        {
            var trimmed = part.Trim();
            if (trimmed.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                return trimmed[3..];
        }
        return null;
    }
}
