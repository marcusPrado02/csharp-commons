using MongoDB.Bson.Serialization.Attributes;

namespace MarcusPrado.Platform.MongoDb.Repository;

/// <summary>Envelope that wraps a document with an explicit <c>_id</c> string.</summary>
public sealed class DocumentEnvelope<T>
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    [BsonElement("data")]
    public T Data { get; set; } = default!;
}
