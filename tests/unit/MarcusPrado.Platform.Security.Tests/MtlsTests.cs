using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MarcusPrado.Platform.Security.Mtls;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Security.Tests;

public sealed class MtlsTests
{
    private static X509Certificate2 CreateSelfSignedCert(string cn)
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest($"CN={cn}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
    }

    // Test 1: CN with tenant prefix resolves tenant ID
    [Fact]
    public void CertificateTenantResolver_CnWithPrefix_ReturnsTenantId()
    {
        var opts = new MtlsOptions();
        var resolver = new CertificateTenantResolver(opts);
        using var cert = CreateSelfSignedCert("tenant:acme-corp");

        var tenantId = resolver.ResolveTenantId(cert);

        tenantId.Should().Be("acme-corp");
    }

    // Test 2: CN without tenant prefix returns null
    [Fact]
    public void CertificateTenantResolver_CnWithoutPrefix_ReturnsNull()
    {
        var opts = new MtlsOptions();
        var resolver = new CertificateTenantResolver(opts);
        using var cert = CreateSelfSignedCert("some-service");

        var tenantId = resolver.ResolveTenantId(cert);

        tenantId.Should().BeNull();
    }

    // Test 3: Null certificate throws ArgumentNullException
    [Fact]
    public void CertificateTenantResolver_NullCertificate_ThrowsArgumentNullException()
    {
        var opts = new MtlsOptions();
        var resolver = new CertificateTenantResolver(opts);

        var act = () => resolver.ResolveTenantId(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // Test 4: CheckRevocation=false never reports revoked
    [Fact]
    public void CertificateRevocationChecker_CheckRevocationFalse_NeverRevoked()
    {
        var opts = new MtlsOptions { CheckRevocation = false };
        var checker = new CertificateRevocationChecker(opts);
        using var cert = CreateSelfSignedCert("test-service");

        var isRevoked = checker.IsRevoked(cert);

        isRevoked.Should().BeFalse();
    }

    // Test 5: MtlsOptions defaults
    [Fact]
    public void MtlsOptions_Defaults_AreCorrect()
    {
        var opts = new MtlsOptions();

        opts.CheckRevocation.Should().BeFalse();
        opts.TenantCnPrefix.Should().Be("tenant:");
        opts.TenantIdSanOid.Should().Be("1.3.6.1.4.1.99999.1");
        opts.RevocationMode.Should().Be(X509RevocationMode.Online);
    }

    // Test 6: AddPlatformMtls registers CertificateTenantResolver in DI
    [Fact]
    public void AddPlatformMtls_RegistersCertificateTenantResolver()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformMtls();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetService<CertificateTenantResolver>();

        resolver.Should().NotBeNull();
    }

    // Test 7: AddPlatformMtls registers authentication scheme "Mtls"
    [Fact]
    public async Task AddPlatformMtls_RegistersMtlsAuthenticationScheme()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPlatformMtls();
        var sp = services.BuildServiceProvider();

        var schemeProvider = sp.GetRequiredService<IAuthenticationSchemeProvider>();
        var scheme = await schemeProvider.GetSchemeAsync(MtlsExtensions.SchemeName);

        scheme.Should().NotBeNull();
        scheme!.Name.Should().Be("Mtls");
    }
}
