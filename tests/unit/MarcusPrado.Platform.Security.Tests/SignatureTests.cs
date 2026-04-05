using System.Net;
using System.Security.Cryptography;
using System.Text;
using MarcusPrado.Platform.Security.Signatures;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace MarcusPrado.Platform.Security.Tests;

public sealed class SignatureTests
{
    // ── RSA ───────────────────────────────────────────────────────────────────

    [Fact]
    public void RsaSignatureService_SignAndVerify_RoundTrip_ReturnsTrue()
    {
        using var svc = RsaSignatureService.CreateEphemeral();
        var data = Encoding.UTF8.GetBytes("hello RSA");

        var signature = svc.Sign(data);
        var result = svc.Verify(data, signature);

        result.Should().BeTrue();
    }

    [Fact]
    public void RsaSignatureService_VerifyTamperedData_ReturnsFalse()
    {
        using var svc = RsaSignatureService.CreateEphemeral();
        var data = Encoding.UTF8.GetBytes("hello RSA");
        var signature = svc.Sign(data);

        var tampered = Encoding.UTF8.GetBytes("tampered RSA");
        var result = svc.Verify(tampered, signature);

        result.Should().BeFalse();
    }

    // ── ECDSA ─────────────────────────────────────────────────────────────────

    [Fact]
    public void EcdsaSignatureService_SignAndVerify_RoundTrip_ReturnsTrue()
    {
        using var svc = EcdsaSignatureService.CreateEphemeral();
        var data = Encoding.UTF8.GetBytes("hello ECDSA");

        var signature = svc.Sign(data);
        var result = svc.Verify(data, signature);

        result.Should().BeTrue();
    }

    [Fact]
    public void EcdsaSignatureService_VerifyWithWrongKey_ReturnsFalse()
    {
        using var svcSigner = EcdsaSignatureService.CreateEphemeral();
        using var svcOther = EcdsaSignatureService.CreateEphemeral();
        var data = Encoding.UTF8.GetBytes("hello ECDSA");

        var signature = svcSigner.Sign(data);
        var result = svcOther.Verify(data, signature);

        result.Should().BeFalse();
    }

    // ── SignedPayloadEnvelope ─────────────────────────────────────────────────

    [Fact]
    public void SignedPayloadEnvelope_IsWithinWindow_RecentTimestamp_ReturnsTrue()
    {
        var envelope = new SignedPayloadEnvelope<string>(
            Payload: "payload",
            Signature: "sig",
            Nonce: "nonce",
            Timestamp: DateTimeOffset.UtcNow.AddSeconds(-10));

        var result = envelope.IsWithinWindow(TimeSpan.FromMinutes(5));

        result.Should().BeTrue();
    }

    [Fact]
    public void SignedPayloadEnvelope_IsWithinWindow_OldTimestamp_ReturnsFalse()
    {
        var envelope = new SignedPayloadEnvelope<string>(
            Payload: "payload",
            Signature: "sig",
            Nonce: "nonce",
            Timestamp: DateTimeOffset.UtcNow.AddMinutes(-10));

        var result = envelope.IsWithinWindow(TimeSpan.FromMinutes(5));

        result.Should().BeFalse();
    }

    // ── WebhookSignatureMiddleware ────────────────────────────────────────────

    private static (TestServer server, byte[] secret) CreateWebhookServer()
    {
        var secret = Encoding.UTF8.GetBytes("super-secret-key");
        var host = new HostBuilder()
            .ConfigureWebHost(web =>
            {
                web.UseTestServer();
                web.Configure(app =>
                {
                    app.UseWebhookSignatureValidation(opts => opts.Secret = secret);
                    app.Run(ctx =>
                    {
                        ctx.Response.StatusCode = 200;
                        return Task.CompletedTask;
                    });
                });
            })
            .Build();
        host.Start();
        return (host.GetTestServer(), secret);
    }

    private static string ComputeHmac(byte[] secret, long unixTs, string body)
    {
        var payload = $"{unixTs}.{body}";
        using var hmac = new HMACSHA256(secret);
        return Convert.ToHexString(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }

    [Fact]
    public async Task WebhookSignatureMiddleware_ValidSignatureAndTimestamp_Returns200()
    {
        var (server, secret) = CreateWebhookServer();
        var client = server.CreateClient();

        var body = "{\"event\":\"test\"}";
        var unixTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sig = ComputeHmac(secret, unixTs, body);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhook")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-Webhook-Timestamp", unixTs.ToString());
        request.Headers.Add("X-Webhook-Signature", sig);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WebhookSignatureMiddleware_InvalidSignature_Returns401()
    {
        var (server, _) = CreateWebhookServer();
        var client = server.CreateClient();

        var body = "{\"event\":\"test\"}";
        var unixTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhook")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-Webhook-Timestamp", unixTs.ToString());
        request.Headers.Add("X-Webhook-Signature", "invalidsignature");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WebhookSignatureMiddleware_TimestampOutsideWindow_Returns401()
    {
        var (server, secret) = CreateWebhookServer();
        var client = server.CreateClient();

        var body = "{\"event\":\"test\"}";
        var unixTs = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds();
        var sig = ComputeHmac(secret, unixTs, body);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhook")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-Webhook-Timestamp", unixTs.ToString());
        request.Headers.Add("X-Webhook-Signature", sig);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
