namespace MarcusPrado.Platform.Domain.SeedWork;

/// <summary>
/// Base class for all exceptions that originate inside the domain layer.
/// Represents a violation of a domain invariant or an unexpected domain condition.
/// Prefer using <see cref="MarcusPrado.Platform.Abstractions.Results.Result{T}"/>
/// for expected failures; reserve exceptions for truly exceptional cases.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>Initialises with a descriptive message.</summary>
    protected DomainException(string message)
        : base(message) { }

    /// <summary>Initialises with a message and an inner cause.</summary>
    protected DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
