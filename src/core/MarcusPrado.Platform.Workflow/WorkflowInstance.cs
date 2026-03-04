namespace MarcusPrado.Platform.Workflow;

/// <summary>
/// Mutable snapshot of a running (or completed) workflow instance.
/// Internal mutations use <c>with</c> expressions on this record.
/// </summary>
public sealed record WorkflowInstance(
    Guid                       Id,
    string                     DefinitionId,
    WorkflowStatus             Status,
    object?                    Context,
    IReadOnlyList<string>      CompletedStepIds,
    DateTimeOffset             StartedAt,
    DateTimeOffset?            CompletedAt,
    string?                    CancellationReason,
    string?                    FailureReason);
