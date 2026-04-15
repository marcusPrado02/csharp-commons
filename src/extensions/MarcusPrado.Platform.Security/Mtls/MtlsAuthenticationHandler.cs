using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MarcusPrado.Platform.Security.Mtls;

public sealed class MtlsAuthenticationOptions : AuthenticationSchemeOptions { }

public sealed class MtlsAuthenticationHandler : AuthenticationHandler<MtlsAuthenticationOptions>
{
    private readonly CertificateTenantResolver _tenantResolver;
    private readonly CertificateRevocationChecker _revocationChecker;

    public MtlsAuthenticationHandler(
        IOptionsMonitor<MtlsAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        CertificateTenantResolver tenantResolver,
        CertificateRevocationChecker revocationChecker)
        : base(options, logger, encoder)
    {
        _tenantResolver = tenantResolver;
        _revocationChecker = revocationChecker;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var clientCert = Context.Connection.ClientCertificate;
        if (clientCert is null)
            return Task.FromResult(AuthenticateResult.Fail("No client certificate provided."));

        if (_revocationChecker.IsRevoked(clientCert))
            return Task.FromResult(AuthenticateResult.Fail("Client certificate has been revoked."));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Thumbprint, clientCert.Thumbprint),
            new(ClaimTypes.Name, clientCert.Subject),
        };

        var tenantId = _tenantResolver.ResolveTenantId(clientCert);
        if (tenantId is not null)
            claims.Add(new Claim("tenant_id", tenantId));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
