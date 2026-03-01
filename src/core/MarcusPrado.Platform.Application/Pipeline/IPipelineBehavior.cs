namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>
/// Marker interface for CQRS pipeline behaviours. Implementations are discovered
/// by <c>AddPlatformCqrs()</c> and registered with the DI container in the order
/// they are declared.
/// </summary>
public interface IPipelineBehavior { }
