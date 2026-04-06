namespace MyDomain.Application.Commands;

/// <summary>Example command.</summary>
public sealed record MyCommand(Guid IdempotencyKey, string Payload);
