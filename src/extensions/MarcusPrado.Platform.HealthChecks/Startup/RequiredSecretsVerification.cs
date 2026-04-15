using Microsoft.Extensions.Configuration;

namespace MarcusPrado.Platform.HealthChecks.Startup;

/// <summary>
/// Verifies that a set of required configuration keys are present and non-empty.
/// </summary>
public sealed class RequiredSecretsVerification : IStartupVerification
{
    private readonly IEnumerable<string> _requiredKeys;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initialises the verification with the keys to check and the configuration source.
    /// </summary>
    /// <param name="requiredKeys">The configuration keys that must have non-null, non-empty values.</param>
    /// <param name="configuration">The application configuration to inspect.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public RequiredSecretsVerification(IEnumerable<string> requiredKeys, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(requiredKeys);
        ArgumentNullException.ThrowIfNull(configuration);

        _requiredKeys = requiredKeys;
        _configuration = configuration;
    }

    /// <inheritdoc/>
    public string Name => "RequiredSecrets";

    /// <inheritdoc/>
    public Task<VerificationResult> VerifyAsync(CancellationToken ct)
    {
        var missing = _requiredKeys.Where(k => string.IsNullOrEmpty(_configuration[k])).ToList();

        if (missing.Count == 0)
            return Task.FromResult(new VerificationResult(true, Name, null));

        var errorMessage = $"Missing or empty required configuration keys: {string.Join(", ", missing)}";
        return Task.FromResult(new VerificationResult(false, Name, errorMessage));
    }
}
