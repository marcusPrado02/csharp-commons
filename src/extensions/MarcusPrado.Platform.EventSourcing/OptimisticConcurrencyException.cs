namespace MarcusPrado.Platform.EventSourcing;

/// <summary>
/// Exception thrown when an event append fails due to a version mismatch, indicating a concurrent write conflict.
/// </summary>
public sealed class OptimisticConcurrencyException : Exception
{
    /// <summary>Gets the identifier of the event stream that caused the conflict.</summary>
    public string StreamId { get; }

    /// <summary>Gets the version the caller expected the stream to be at.</summary>
    public long ExpectedVersion { get; }

    /// <summary>Gets the actual version of the stream at the time of the conflict.</summary>
    public long ActualVersion { get; }

    /// <summary>
    /// Initializes a new instance with details about the conflicting stream versions.
    /// </summary>
    /// <param name="streamId">The identifier of the event stream that caused the conflict.</param>
    /// <param name="expectedVersion">The version the caller expected.</param>
    /// <param name="actualVersion">The version actually found in the store.</param>
    public OptimisticConcurrencyException(string streamId, long expectedVersion, long actualVersion)
        : base($"Stream '{streamId}': expected version {expectedVersion} but found {actualVersion}.")
    {
        StreamId = streamId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
}
