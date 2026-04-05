namespace MarcusPrado.Platform.EventSourcing;

public sealed class OptimisticConcurrencyException : Exception
{
    public string StreamId { get; }

    public long ExpectedVersion { get; }

    public long ActualVersion { get; }

    public OptimisticConcurrencyException(string streamId, long expectedVersion, long actualVersion)
        : base($"Stream '{streamId}': expected version {expectedVersion} but found {actualVersion}.")
    {
        StreamId = streamId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
}
