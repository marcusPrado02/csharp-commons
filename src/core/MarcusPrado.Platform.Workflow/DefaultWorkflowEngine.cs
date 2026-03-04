using System.Collections.Concurrent;

namespace MarcusPrado.Platform.Workflow;

/// <summary>
/// In-memory, thread-safe workflow engine with Saga compensation support.
/// </summary>
public sealed class DefaultWorkflowEngine : IWorkflowEngine
{
    private readonly ConcurrentDictionary<string, WorkflowDefinition> _definitions = new();
    private readonly ConcurrentDictionary<Guid, WorkflowInstance> _instances = new();

    /// <inheritdoc />
    public void RegisterDefinition(WorkflowDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _definitions[definition.Id] = definition;
    }

    /// <inheritdoc />
    public async Task<Result<WorkflowInstance>> StartWorkflowAsync(
        string definitionId,
        object? initialContext = null,
        CancellationToken ct = default)
    {
        if (!_definitions.TryGetValue(definitionId, out var def))
        {
            return Error.NotFound(
                "WORKFLOW.DEFINITION_NOT_FOUND",
                $"Workflow definition '{definitionId}' was not found.");
        }

        var instance = new WorkflowInstance(
            Id: Guid.NewGuid(),
            DefinitionId: definitionId,
            Status: WorkflowStatus.Running,
            Context: initialContext,
            CompletedStepIds: [],
            StartedAt: DateTimeOffset.UtcNow,
            CompletedAt: null,
            CancellationReason: null,
            FailureReason: null);

        var completedSteps = new List<string>();
        var ctx = initialContext;

        foreach (var step in def.Steps)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                ctx = await step.Execute(ctx, ct).ConfigureAwait(false);
                completedSteps.Add(step.Id);
                instance = instance with { CompletedStepIds = completedSteps.ToArray(), Context = ctx };
                _instances[instance.Id] = instance;
            }
#pragma warning disable CA1031 // Intentional: any step failure must be captured as a workflow error
            catch (Exception ex)
            {
                instance = instance with
                {
                    Status = WorkflowStatus.Failed,
                    FailureReason = ex.Message,
                    CompletedAt = DateTimeOffset.UtcNow,
                };
                _instances[instance.Id] = instance;
                return Error.Technical("WORKFLOW.STEP_FAILED", ex.Message);
            }
#pragma warning restore CA1031
        }

        instance = instance with
        {
            Status = WorkflowStatus.Completed,
            CompletedAt = DateTimeOffset.UtcNow,
        };
        _instances[instance.Id] = instance;
        return instance;
    }

    /// <inheritdoc />
    public Task<Result<WorkflowInstance>> SendEventAsync(
        Guid workflowId,
        string eventName,
        object? eventData = null,
        CancellationToken ct = default)
    {
        if (!_instances.TryGetValue(workflowId, out var instance))
        {
            return Task.FromResult<Result<WorkflowInstance>>(
                Error.NotFound(
                    "WORKFLOW.INSTANCE_NOT_FOUND",
                    $"Workflow instance '{workflowId}' was not found."));
        }

        var updated = instance with
        {
            Status = WorkflowStatus.Running,
            Context = eventData ?? instance.Context,
        };
        _instances[workflowId] = updated;
        return Task.FromResult<Result<WorkflowInstance>>(updated);
    }

    /// <inheritdoc />
    public async Task<Result<WorkflowInstance>> CompensateAsync(
        Guid workflowId,
        CancellationToken ct = default)
    {
        if (!_instances.TryGetValue(workflowId, out var instance))
        {
            return Error.NotFound(
                "WORKFLOW.INSTANCE_NOT_FOUND",
                $"Workflow instance '{workflowId}' was not found.");
        }

        if (!_definitions.TryGetValue(instance.DefinitionId, out var def))
        {
            return Error.Technical("WORKFLOW.DEFINITION_MISSING", "Definition no longer registered.");
        }

        instance = instance with { Status = WorkflowStatus.Compensating };
        _instances[workflowId] = instance;

        foreach (var stepId in instance.CompletedStepIds.Reverse())
        {
            var step = def.Steps.FirstOrDefault(s => s.Id == stepId);
            if (step?.Compensate is not null)
            {
                try
                {
                    await step.Compensate(instance.Context, ct).ConfigureAwait(false);
                }
#pragma warning disable CA1031,RCS1075 // Intentional: best-effort compensation must never throw
                catch (Exception)
                {
                    // deliberately suppressed — compensation is best-effort
                }
#pragma warning restore CA1031,RCS1075
            }
        }

        instance = instance with
        {
            Status = WorkflowStatus.Compensated,
            CompletedAt = DateTimeOffset.UtcNow,
        };
        _instances[workflowId] = instance;
        return instance;
    }

    /// <inheritdoc />
    public Task<Result<WorkflowInstance>> CancelAsync(
        Guid workflowId,
        string reason = "",
        CancellationToken ct = default)
    {
        if (!_instances.TryGetValue(workflowId, out var instance))
        {
            return Task.FromResult<Result<WorkflowInstance>>(
                Error.NotFound(
                    "WORKFLOW.INSTANCE_NOT_FOUND",
                    $"Workflow instance '{workflowId}' was not found."));
        }

        var cancelled = instance with
        {
            Status = WorkflowStatus.Cancelled,
            CancellationReason = reason,
            CompletedAt = DateTimeOffset.UtcNow,
        };
        _instances[workflowId] = cancelled;
        return Task.FromResult<Result<WorkflowInstance>>(cancelled);
    }

    /// <inheritdoc />
    public Task<WorkflowInstance?> GetInstanceAsync(Guid workflowId, CancellationToken ct = default)
        => Task.FromResult(_instances.TryGetValue(workflowId, out var i) ? i : null);
}
