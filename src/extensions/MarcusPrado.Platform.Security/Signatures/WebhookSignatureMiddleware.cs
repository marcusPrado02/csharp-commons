using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace MarcusPrado.Platform.Security.Signatures;

public sealed class WebhookSignatureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly WebhookSignatureOptions _options;

    public WebhookSignatureMiddleware(RequestDelegate next, WebhookSignatureOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Read timestamp
        if (!context.Request.Headers.TryGetValue(_options.TimestampHeader, out var tsHeader)
            || !long.TryParse(tsHeader, out var unixTs))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var requestTime = DateTimeOffset.FromUnixTimeSeconds(unixTs);
        if (DateTimeOffset.UtcNow - requestTime > _options.TimestampWindow)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // 2. Read body
        context.Request.EnableBuffering();
        var body = await new StreamReader(context.Request.Body, leaveOpen: true)
            .ReadToEndAsync();
        context.Request.Body.Position = 0;

        // 3. Compute expected HMAC
        var payload = $"{unixTs}.{body}";
        using var hmac = new HMACSHA256(_options.Secret);
        var expected = Convert.ToHexString(
            hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();

        // 4. Compare
        if (!context.Request.Headers.TryGetValue(_options.HeaderName, out var sigHeader)
            || !CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(sigHeader.ToString()),
                System.Text.Encoding.UTF8.GetBytes(expected)))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await _next(context);
    }
}
