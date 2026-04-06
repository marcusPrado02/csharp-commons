namespace MarcusPrado.Platform.ContractTestKit.Pact;

/// <summary>
/// Represents the result of verifying a single Pact interaction.
/// </summary>
/// <param name="Interaction">The name or description of the interaction being verified.</param>
/// <param name="Success">Whether the interaction verification succeeded.</param>
/// <param name="ErrorMessage">The error message if verification failed; <see langword="null"/> otherwise.</param>
public record ContractVerificationResult(string Interaction, bool Success, string? ErrorMessage);
