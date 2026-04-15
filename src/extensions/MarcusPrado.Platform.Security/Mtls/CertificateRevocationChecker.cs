using System.Security.Cryptography.X509Certificates;

namespace MarcusPrado.Platform.Security.Mtls;

public sealed class CertificateRevocationChecker
{
    private readonly MtlsOptions _options;

    public CertificateRevocationChecker(MtlsOptions options) => _options = options;

    /// <summary>Returns true if the certificate has been revoked, false if valid.</summary>
    public bool IsRevoked(X509Certificate2 certificate, X509Certificate2Collection? chain = null)
    {
        if (!_options.CheckRevocation)
            return false;

        using var certChain = new X509Chain();
        certChain.ChainPolicy.RevocationMode = _options.RevocationMode;
        certChain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;

        if (chain is not null)
            certChain.ChainPolicy.ExtraStore.AddRange(chain);

        var isValid = certChain.Build(certificate);
        if (isValid)
            return false;

        return certChain.ChainStatus.Any(s => s.Status.HasFlag(X509ChainStatusFlags.Revoked));
    }
}
