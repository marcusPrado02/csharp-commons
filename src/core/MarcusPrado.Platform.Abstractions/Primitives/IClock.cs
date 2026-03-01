namespace MarcusPrado.Platform.Abstractions.Primitives;

/// <summary>
/// Abstraction over the system clock, enabling deterministic time in tests.
/// </summary>
public interface IClock
{
    /// <summary>Gets the current date and time expressed as UTC.</summary>
    DateTimeOffset UtcNow { get; }
}
