using MongoDB.Bson.Serialization.Attributes;

namespace MarcusPrado.Platform.MongoDb.Repository;

/// <summary>Envelope that wraps a document with an explicit <c>_id</c> string.</summary>
public sealed class DocumentEnvelope<T>
{
    /// <summary>The MongoDB document identifier stored in the <c>_id</c> field.</summary>
    [BsonId]
    public string Id { get; set; } = string.Empty;

    /// <summary>The wrapped domain document stored in the <c>data</c> field.</summary>
    [BsonElement("data")]
    public T Data { get; set; } = default!;
}
