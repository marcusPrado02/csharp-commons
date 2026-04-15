namespace MarcusPrado.Platform.AspNetCore.Auth.Tests.Helpers;

/// <summary>
/// Generates signed JWT tokens for testing.
/// </summary>
public static class JwtTokenFactory
{
    public const string TestSigningKey = "super-secret-key-that-is-long-enough-32chars";
    public const string TestIssuer = "platform-tests";
    public const string TestAudience = "platform-api";

    private static SymmetricSecurityKey SigningKey => new(Encoding.UTF8.GetBytes(TestSigningKey));

    private static readonly JsonWebTokenHandler TokenHandler = new();

    /// <summary>Creates a valid, signed JWT with the provided claims.</summary>
    public static string CreateValidToken(
        string? subject = "user-123",
        string? issuer = TestIssuer,
        string? audience = TestAudience,
        IEnumerable<Claim>? extraClaims = null,
        TimeSpan? expiresIn = null
    )
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>();

        if (subject is not null)
            claims.Add(new Claim("sub", subject));

        if (extraClaims is not null)
            claims.AddRange(extraClaims);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = issuer,
            Audience = audience,
            NotBefore = now,
            Expires = now.Add(expiresIn ?? TimeSpan.FromHours(1)),
            SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256),
        };

        return TokenHandler.CreateToken(descriptor);
    }

    /// <summary>Creates a token signed with a DIFFERENT key (invalid signature).</summary>
    public static string CreateTokenWithWrongKey()
    {
        var wrongKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("wrong-key-also-needs-to-be-long-enough!!"));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("sub", "user-evil")]),
            Issuer = TestIssuer,
            Audience = TestAudience,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(wrongKey, SecurityAlgorithms.HmacSha256),
        };
        return TokenHandler.CreateToken(descriptor);
    }

    /// <summary>Creates an already-expired token.</summary>
    public static string CreateExpiredToken() => CreateValidToken(expiresIn: TimeSpan.FromSeconds(-1));
}
