namespace MyDomain.Application.Commands;

/// <summary>Handles <see cref="MyCommand"/>.</summary>
public sealed class MyCommandHandler
{
    /// <summary>Handles the command.</summary>
    public Task HandleAsync(MyCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        return Task.CompletedTask;
    }
}
