namespace MarcusPrado.Platform.Application.Tests;

/// <summary>Simple validation result used in tests.</summary>
public sealed class TestValidationResult : IValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<Error> Errors { get; }

    public TestValidationResult(bool isValid, IReadOnlyList<Error>? errors = null)
    {
        IsValid = isValid;
        Errors = errors ?? [];
    }
}
