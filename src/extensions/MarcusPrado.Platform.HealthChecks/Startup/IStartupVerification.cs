namespace MarcusPrado.Platform.HealthChecks.Startup;

/// <summary>
/// Contract for a single startup verification step.
/// </summary>
public interface IStartupVerification
{
    /// <summary>The unique name that identifies this verification.</summary>
    string Name { get; }

    /// <summary>Executes the verification and returns its result.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="VerificationResult"/> describing success or failure.</returns>
    Task<VerificationResult> VerifyAsync(CancellationToken ct);
}
