namespace MarcusPrado.Platform.EventSourcing.Saga;

/// <summary>
/// Defines a saga with a typed state and an ordered list of steps.
/// </summary>
/// <typeparam name="TState">The type that holds the saga's mutable state.</typeparam>
public interface ISaga<TState>
{
    /// <summary>Gets the unique identifier for this saga instance.</summary>
    string Id { get; }

    /// <summary>Gets the current state of the saga.</summary>
    TState State { get; }

    /// <summary>Gets the current execution status of the saga.</summary>
    SagaStatus Status { get; set; }

    /// <summary>Gets the ordered list of steps to execute.</summary>
    IReadOnlyList<ISagaStepDescriptor> Steps { get; }
}
