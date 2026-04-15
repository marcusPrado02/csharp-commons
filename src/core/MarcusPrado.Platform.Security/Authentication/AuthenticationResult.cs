namespace MarcusPrado.Platform.Security.Authentication;

/// <summary>
/// Represents the outcome of a token-validation attempt.
/// </summary>
public sealed class AuthenticationResult
{
    /// <summary>Gets a value indicating whether the token was valid and unexpired.</summary>
    public bool IsAuthenticated { get; private init; }

    /// <summary>Gets the subject (user ID) extracted from the token, if authentication succeeded.</summary>
    public string? UserId { get; private init; }

    /// <summary>Gets the claims extracted from the token as a flat dictionary (type → first value).</summary>
    public IReadOnlyDictionary<string, string> Claims { get; private init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the failure reason when <see cref="IsAuthenticated"/> is <c>false</c>.</summary>
    public string? FailureReason { get; private init; }

    /// <summary>Creates a successful authentication result.</summary>
    public static AuthenticationResult Success(string? userId, IReadOnlyDictionary<string, string> claims) =>
        new()
        {
            IsAuthenticated = true,
            UserId = userId,
            Claims = claims,
        };

    /// <summary>Creates a failed authentication result with a reason.</summary>
    public static AuthenticationResult Fail(string reason) => new() { IsAuthenticated = false, FailureReason = reason };
}
