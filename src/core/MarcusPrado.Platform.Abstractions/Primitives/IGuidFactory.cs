namespace MarcusPrado.Platform.Abstractions.Primitives;

/// <summary>
/// Abstraction over GUID creation to enable sequential IDs and testability.
/// </summary>
public interface IGuidFactory
{
    /// <summary>Creates and returns a new unique identifier.</summary>
    Guid NewGuid();
}
