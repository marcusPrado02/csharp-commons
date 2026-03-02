namespace MarcusPrado.Platform.Security.Authentication;

/// <summary>
/// Validates raw security tokens (JWT, opaque, etc.) and returns a structured
/// <see cref="AuthenticationResult"/>.
/// </summary>
public interface ITokenValidator
{
    /// <summary>
    /// Validates <paramref name="token"/> and returns an <see cref="AuthenticationResult"/>
    /// indicating success or failure with optional claim data.
    /// </summary>
    AuthenticationResult Validate(string token);
}
