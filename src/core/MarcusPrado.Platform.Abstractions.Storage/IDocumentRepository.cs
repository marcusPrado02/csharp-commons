namespace MarcusPrado.Platform.Abstractions.Storage;

/// <summary>Generic document-store repository abstraction (MongoDB-compatible).</summary>
/// <typeparam name="T">The document type.</typeparam>
public interface IDocumentRepository<T>
    where T : class
{
    /// <summary>Returns the document with the given ID, or <see langword="null"/>.</summary>
    Task<T?> FindByIdAsync(string id, CancellationToken ct = default);

    /// <summary>Returns all documents in the collection.</summary>
    Task<IReadOnlyList<T>> FindAllAsync(CancellationToken ct = default);

    /// <summary>Returns documents that satisfy the given predicate.</summary>
    Task<IReadOnlyList<T>> FindAsync(Func<T, bool> predicate, CancellationToken ct = default);

    /// <summary>Inserts a new document with the given ID.</summary>
    Task InsertAsync(string id, T document, CancellationToken ct = default);

    /// <summary>Replaces the document with the given ID.</summary>
    Task ReplaceAsync(string id, T document, CancellationToken ct = default);

    /// <summary>Deletes the document with the given ID.</summary>
    Task DeleteAsync(string id, CancellationToken ct = default);

    /// <summary>Returns the total number of documents in the collection.</summary>
    Task<long> CountAsync(CancellationToken ct = default);
}

/// <summary>Options for connecting to a MongoDB-compatible document store.</summary>
public sealed record DocumentStoreOptions(string ConnectionString, string DatabaseName, string? AppName = null);
