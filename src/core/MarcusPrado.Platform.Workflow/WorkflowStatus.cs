namespace MarcusPrado.Platform.Workflow;

/// <summary>The current lifecycle state of a workflow instance.</summary>
public enum WorkflowStatus
{
    /// <summary>Workflow was created but not yet started.</summary>
    Pending,

    /// <summary>Workflow is actively running.</summary>
    Running,

    /// <summary>Workflow is paused waiting for an external event.</summary>
    WaitingForEvent,

    /// <summary>Workflow completed successfully.</summary>
    Completed,

    /// <summary>Workflow was cancelled.</summary>
    Cancelled,

    /// <summary>Workflow failed and compensation has been triggered.</summary>
    Compensating,

    /// <summary>Compensation finished.</summary>
    Compensated,

    /// <summary>Workflow failed with unrecoverable error.</summary>
    Failed,
}
