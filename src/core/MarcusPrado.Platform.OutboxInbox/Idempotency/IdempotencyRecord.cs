namespace MarcusPrado.Platform.OutboxInbox.Idempotency;

/// <summary>Stores the result of a previously-executed idempotent operation.</summary>
public sealed class IdempotencyRecord
{
    /// <summary>Gets or sets the idempotency key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets when this record was first created.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets when this record expires (null = never).</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>Gets or sets the serialised result payload, if any.</summary>
    public string? ResultPayload { get; set; }

    /// <summary>Returns true if this record has not yet expired.</summary>
    public bool IsValid(DateTimeOffset now) =>
        ExpiresAt is null || ExpiresAt > now;
}
