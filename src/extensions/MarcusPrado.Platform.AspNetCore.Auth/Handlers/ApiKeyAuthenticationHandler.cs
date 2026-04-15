using MarcusPrado.Platform.Abstractions.Context;
using MarcusPrado.Platform.AspNetCore.Auth.Options;

namespace MarcusPrado.Platform.AspNetCore.Auth.Handlers;

/// <summary>
/// ASP.NET Core <see cref="AuthenticationHandler{T}"/> that validates an
/// API key sent via a configurable HTTP header (default: <c>X-Api-Key</c>).
/// </summary>
public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headerName = Options.HeaderName;

        if (!Request.Headers.TryGetValue(headerName, out var headerValues))
            return Task.FromResult(AuthenticateResult.NoResult());

        var apiKey = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(apiKey))
            return Task.FromResult(AuthenticateResult.Fail("Empty API key."));

        if (!Options.ValidKeys.Contains(apiKey))
        {
            Logger.LogWarning("Invalid API key presented.");
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        // Build a minimal principal representing the API client
        var claims = new[] { new Claim("apikey", apiKey), new Claim(ClaimTypes.Name, "api-client") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);

        var userContext = Context.RequestServices.GetRequiredService<IUserContext>();
        userContext.SetUser(principal);

        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
