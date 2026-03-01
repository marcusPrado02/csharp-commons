namespace MarcusPrado.Platform.Abstractions.Errors;

/// <summary>
/// Provides a registry of known application <see cref="Error"/> instances,
/// enabling centralised error definitions and look-up by code.
/// </summary>
public interface IErrorCatalog
{
    /// <summary>Returns the <see cref="Error"/> with the given <paramref name="code"/>, or <c>null</c> if not registered.</summary>
    Error? GetByCode(string code);

    /// <summary>Returns all registered errors.</summary>
    IEnumerable<Error> GetAll();

    /// <summary>Returns all registered errors that belong to <paramref name="category"/>.</summary>
    IEnumerable<Error> GetByCategory(ErrorCategory category);
}
