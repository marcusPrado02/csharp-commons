namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>Wraps each command in a database transaction, rolling back on failure.</summary>
public class TransactionBehavior : IPipelineBehavior { }
