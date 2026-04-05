namespace MarcusPrado.Platform.EventSourcing.Saga;

/// <summary>
/// Represents the lifecycle status of a saga.
/// </summary>
public enum SagaStatus
{
    /// <summary>The saga is currently executing steps.</summary>
    Running,

    /// <summary>All steps completed successfully.</summary>
    Completed,

    /// <summary>A step failed and compensations are being executed.</summary>
    Compensating,

    /// <summary>The saga has failed (and compensation may have run).</summary>
    Failed
}
