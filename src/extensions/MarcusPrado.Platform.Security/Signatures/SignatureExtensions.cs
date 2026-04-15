using Microsoft.AspNetCore.Builder;

namespace MarcusPrado.Platform.Security.Signatures;

public static class SignatureExtensions
{
    public static IApplicationBuilder UseWebhookSignatureValidation(
        this IApplicationBuilder app,
        Action<WebhookSignatureOptions>? configure = null
    )
    {
        var opts = new WebhookSignatureOptions();
        configure?.Invoke(opts);
        return app.UseMiddleware<WebhookSignatureMiddleware>(opts);
    }
}
