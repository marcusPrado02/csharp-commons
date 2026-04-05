using System.Security.Cryptography.X509Certificates;

namespace MarcusPrado.Platform.Security.Mtls;

public sealed class MtlsOptions
{
    /// <summary>SAN (Subject Alternative Name) extension OID used for tenant ID. Default: 1.3.6.1.4.1.99999.1 (custom OID).</summary>
    public string TenantIdSanOid { get; set; } = "1.3.6.1.4.1.99999.1";

    /// <summary>CN field prefix that identifies a tenant. E.g., "tenant:" → "tenant:acme-corp".</summary>
    public string TenantCnPrefix { get; set; } = "tenant:";

    /// <summary>Whether to check certificate revocation (CRL/OCSP). Default: false (for performance; enable in prod).</summary>
    public bool CheckRevocation { get; set; }

    /// <summary>Revocation mode. Ignored if CheckRevocation is false.</summary>
    public X509RevocationMode RevocationMode { get; set; } = X509RevocationMode.Online;
}
