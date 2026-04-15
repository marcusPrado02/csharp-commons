namespace MarcusPrado.Platform.Persistence.Concurrency;

/// <summary>Thrown when an optimistic concurrency check fails because another writer modified the same record.</summary>
public class OptimisticConcurrencyException : Exception { }
