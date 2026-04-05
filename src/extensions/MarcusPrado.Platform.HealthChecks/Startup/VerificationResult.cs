namespace MarcusPrado.Platform.HealthChecks.Startup;

/// <summary>
/// The result produced by an <see cref="IStartupVerification"/> step.
/// </summary>
/// <param name="Success">Whether the verification passed.</param>
/// <param name="Name">The name of the verification that produced this result.</param>
/// <param name="ErrorMessage">An optional error message when <see cref="Success"/> is <c>false</c>.</param>
public sealed record VerificationResult(bool Success, string Name, string? ErrorMessage);
