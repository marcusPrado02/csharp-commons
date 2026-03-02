namespace MarcusPrado.Platform.AspNetCore.Auth;

/// <summary>
/// Well-known authentication scheme names used by the platform's auth stack.
/// Pass these names when configuring <c>AddAuthentication</c> or
/// when decorating endpoints with <c>[Authorize(AuthenticationSchemes = …)]</c>.
/// </summary>
public static class PlatformAuthSchemes
{
    /// <summary>JWT Bearer authentication scheme name.</summary>
    public const string Jwt = "Platform.Jwt";

    /// <summary>API-key header authentication scheme name.</summary>
    public const string ApiKey = "Platform.ApiKey";
}
