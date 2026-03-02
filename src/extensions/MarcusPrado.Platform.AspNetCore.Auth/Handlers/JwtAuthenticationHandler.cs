using MarcusPrado.Platform.Abstractions.Context;
using MarcusPrado.Platform.AspNetCore.Auth.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MarcusPrado.Platform.AspNetCore.Auth.Handlers;

/// <summary>
/// ASP.NET Core <see cref="AuthenticationHandler{T}"/> that validates Bearer
/// JWT tokens and populates <see cref="IUserContext"/> for the current request.
/// </summary>
public sealed class JwtAuthenticationHandler(
    IOptionsMonitor<JwtAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<JwtAuthenticationOptions>(options, logger, encoder)
{
    private const string BearerPrefix = "Bearer ";

    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var headerValues))
            return AuthenticateResult.NoResult();

        var header = headerValues.ToString();
        if (!header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = header[BearerPrefix.Length..].Trim();
        if (string.IsNullOrEmpty(token))
            return AuthenticateResult.Fail("Empty Bearer token.");

        var opts = Options;
        var key  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.SigningKey));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = key,
            ValidateIssuer           = !string.IsNullOrEmpty(opts.Issuer),
            ValidIssuer              = opts.Issuer,
            ValidateAudience         = !string.IsNullOrEmpty(opts.Audience),
            ValidAudience            = opts.Audience,
            ValidateLifetime         = opts.ValidateLifetime,
            ClockSkew                = TimeSpan.Zero,
        };

        try
        {
            var handler    = new JsonWebTokenHandler();
            var result     = await handler.ValidateTokenAsync(token, validationParameters);

            if (!result.IsValid)
                return AuthenticateResult.Fail(result.Exception?.Message ?? "Token validation failed.");

            var identity  = new ClaimsIdentity(result.ClaimsIdentity.Claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            // Populate the request-scoped IUserContext
            var userContext = Context.RequestServices.GetRequiredService<IUserContext>();
            userContext.SetUser(principal);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (SecurityTokenExpiredException)
        {
            return AuthenticateResult.Fail("Token has expired.");
        }
        catch (SecurityTokenException ex)
        {
            Logger.LogWarning(ex, "JWT token validation failed.");
            return AuthenticateResult.Fail("Token validation failed.");
        }
#pragma warning disable CA1031 // Authentication handlers must not surface unexpected exceptions to callers
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during JWT authentication.");
            return AuthenticateResult.Fail("Authentication error.");
        }
#pragma warning restore CA1031
    }
}
