using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Application.Tests;

[Idempotent(60)]
public sealed class IdempotentCommand : ICommand<string>, IHaveIdempotencyKey
{
    public string Payload { get; }
    public string IdempotencyKey { get; }

    public IdempotentCommand(string payload, string key)
    {
        Payload = payload;
        IdempotencyKey = key;
    }
}

[Transactional]
public sealed class TransactionalCommand : ICommand<int> { }

public sealed class SimpleCommand : ICommand<string> { }

public sealed class SimpleQuery : IQuery<string> { }

/// <summary>Void command (no generic result).</summary>
public sealed class VoidCommand : ICommand { }
