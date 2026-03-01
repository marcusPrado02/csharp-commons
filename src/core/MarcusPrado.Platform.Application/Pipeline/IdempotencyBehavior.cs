namespace MarcusPrado.Platform.Application.Pipeline;

/// <summary>Deduplicates commands that carry a client-supplied idempotency key.</summary>
public class IdempotencyBehavior : IPipelineBehavior { }
