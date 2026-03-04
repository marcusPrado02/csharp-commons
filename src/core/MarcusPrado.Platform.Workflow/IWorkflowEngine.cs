namespace MarcusPrado.Platform.Workflow;

/// <summary>Runs and manages workflow instances.</summary>
public interface IWorkflowEngine
{
    /// <summary>Starts a new workflow instance for the given definition.</summary>
    Task<Result<WorkflowInstance>> StartWorkflowAsync(
        string             definitionId,
        object?            initialContext = null,
        CancellationToken  ct             = default);

    /// <summary>Sends an event to a running workflow instance.</summary>
    Task<Result<WorkflowInstance>> SendEventAsync(
        Guid               workflowId,
        string             eventName,
        object?            eventData      = null,
        CancellationToken  ct             = default);

    /// <summary>Compensates (rolls back) all completed steps of a workflow.</summary>
    Task<Result<WorkflowInstance>> CompensateAsync(
        Guid               workflowId,
        CancellationToken  ct = default);

    /// <summary>Cancels a running workflow instance.</summary>
    Task<Result<WorkflowInstance>> CancelAsync(
        Guid               workflowId,
        string             reason         = "",
        CancellationToken  ct             = default);

    /// <summary>Returns the current state of a workflow instance, or <see langword="null"/> if not found.</summary>
    Task<WorkflowInstance?> GetInstanceAsync(Guid workflowId, CancellationToken ct = default);
}
