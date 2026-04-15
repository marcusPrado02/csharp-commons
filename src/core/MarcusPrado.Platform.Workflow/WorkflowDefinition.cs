namespace MarcusPrado.Platform.Workflow;

/// <summary>
/// Immutable descriptor of a workflow — the blueprint used to create instances.
/// </summary>
public sealed record WorkflowDefinition(
    string Id,
    string Name,
    IReadOnlyList<WorkflowStep> Steps,
    string? Description = null);

/// <summary>A single step within a workflow definition.</summary>
public sealed record WorkflowStep(
    string Id,
    string Name,
    Func<object?, CancellationToken, Task<object?>> Execute,
    Func<object?, CancellationToken, Task>? Compensate = null);
