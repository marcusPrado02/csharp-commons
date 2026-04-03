using MarcusPrado.Platform.Abstractions.Storage;
using MongoDB.Driver;

namespace MarcusPrado.Platform.MongoDb.Repository;

/// <summary>
/// MongoDB implementation of <see cref="IDocumentRepository{T}"/>.
/// Documents are stored in a collection named after the document type (lower-case).
/// </summary>
/// <typeparam name="T">The document type.</typeparam>
public sealed class MongoDocumentRepository<T> : IDocumentRepository<T>
    where T : class
{
    private readonly IMongoCollection<DocumentEnvelope<T>> _collection;

    /// <summary>Initializes a new instance of <see cref="MongoDocumentRepository{T}"/>.</summary>
    public MongoDocumentRepository(IMongoDatabase database, string? collectionName = null)
    {
        ArgumentNullException.ThrowIfNull(database);

        var name = collectionName ?? typeof(T).Name.ToLowerInvariant();
        _collection = database.GetCollection<DocumentEnvelope<T>>(name);
    }

    /// <inheritdoc />
    public async Task<T?> FindByIdAsync(string id, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var filter = Builders<DocumentEnvelope<T>>.Filter.Eq(e => e.Id, id);
        var envelope = await _collection.Find(filter).FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        return envelope?.Data;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> FindAllAsync(CancellationToken ct = default)
    {
        var envelopes = await _collection.Find(Builders<DocumentEnvelope<T>>.Filter.Empty)
            .ToListAsync(ct).ConfigureAwait(false);

        return envelopes.Select(e => e.Data).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> FindAsync(
        Func<T, bool> predicate, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var all = await FindAllAsync(ct).ConfigureAwait(false);
        return all.Where(predicate).ToList();
    }

    /// <inheritdoc />
    public async Task InsertAsync(string id, T document, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(document);

        var envelope = new DocumentEnvelope<T> { Id = id, Data = document };
        await _collection.InsertOneAsync(envelope, cancellationToken: ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ReplaceAsync(string id, T document, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(document);

        var filter = Builders<DocumentEnvelope<T>>.Filter.Eq(e => e.Id, id);
        var envelope = new DocumentEnvelope<T> { Id = id, Data = document };
        await _collection.ReplaceOneAsync(filter, envelope, cancellationToken: ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var filter = Builders<DocumentEnvelope<T>>.Filter.Eq(e => e.Id, id);
        await _collection.DeleteOneAsync(filter, cancellationToken: ct)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<long> CountAsync(CancellationToken ct = default)
    {
        return await _collection.CountDocumentsAsync(
            Builders<DocumentEnvelope<T>>.Filter.Empty, cancellationToken: ct)
            .ConfigureAwait(false);
    }
}
