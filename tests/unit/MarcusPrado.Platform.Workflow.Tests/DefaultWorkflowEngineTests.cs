namespace MarcusPrado.Platform.Workflow.Tests;

public sealed class DefaultWorkflowEngineTests
{
    private static WorkflowDefinition SimpleDefinition(string id = "order.process") =>
        new(
            Id: id,
            Name: "Order Process",
            Steps:
            [
                new WorkflowStep(
                    Id:      "validate",
                    Name:    "Validate order",
                    Execute: (ctx, _) => Task.FromResult<object?>(ctx)),
                new WorkflowStep(
                    Id:          "charge",
                    Name:        "Charge customer",
                    Execute:     (ctx, _) => Task.FromResult<object?>(new { Charged = true }),
                    Compensate:  (_, _) => Task.CompletedTask),
            ]);

    private static DefaultWorkflowEngine Engine(WorkflowDefinition? def = null)
    {
        var e = new DefaultWorkflowEngine();
        e.RegisterDefinition(def ?? SimpleDefinition());
        return e;
    }

    [Fact]
    public async Task StartWorkflow_HappyPath_ReturnsCompleted()
    {
        var result = await Engine().StartWorkflowAsync("order.process");
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(WorkflowStatus.Completed);
    }

    [Fact]
    public async Task StartWorkflow_UnknownDefinition_ReturnsFailure()
    {
        var result = await Engine().StartWorkflowAsync("unknown.def");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task StartWorkflow_CompletedStepIds_ContainsAllSteps()
    {
        var result = await Engine().StartWorkflowAsync("order.process");
        result.Value.CompletedStepIds.Should().Contain(["validate", "charge"]);
    }

    [Fact]
    public async Task StartWorkflow_StepThrows_ReturnsFailure()
    {
        var def = new WorkflowDefinition("fail.wf", "Fail WF",
            [new WorkflowStep("boom", "Boom", (_, _) => throw new InvalidOperationException("explode"))]);
        var engine = new DefaultWorkflowEngine();
        engine.RegisterDefinition(def);

        var result = await engine.StartWorkflowAsync("fail.wf");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetInstance_AfterStart_ReturnsInstance()
    {
        var engine = Engine();
        var result = await engine.StartWorkflowAsync("order.process");
        var fetched = await engine.GetInstanceAsync(result.Value.Id);
        fetched.Should().NotBeNull();
        fetched!.Status.Should().Be(WorkflowStatus.Completed);
    }

    [Fact]
    public async Task GetInstance_Missing_ReturnsNull()
    {
        var result = await Engine().GetInstanceAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task CancelWorkflow_InProgress_ReturnsCancelled()
    {
        var engine = Engine();
        var start = await engine.StartWorkflowAsync("order.process");
        var cancel = await engine.CancelAsync(start.Value.Id, "user requested");
        cancel.Value.Status.Should().Be(WorkflowStatus.Cancelled);
        cancel.Value.CancellationReason.Should().Be("user requested");
    }

    [Fact]
    public async Task CancelWorkflow_UnknownId_ReturnsFailure()
    {
        var cancel = await Engine().CancelAsync(Guid.NewGuid());
        cancel.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CompensateWorkflow_CompletedSteps_RunsCompensation()
    {
        var compensated = false;
        var def = new WorkflowDefinition("comp.wf", "Comp WF",
        [
            new WorkflowStep("step1", "S1",
                Execute:    (ctx, _) => Task.FromResult<object?>(ctx),
                Compensate: (_, _) => { compensated = true; return Task.CompletedTask; }),
        ]);
        var engine = new DefaultWorkflowEngine();
        engine.RegisterDefinition(def);
        var start = await engine.StartWorkflowAsync("comp.wf");
        var result = await engine.CompensateAsync(start.Value.Id);

        result.Value.Status.Should().Be(WorkflowStatus.Compensated);
        compensated.Should().BeTrue();
    }

    [Fact]
    public async Task CompensateWorkflow_UnknownId_ReturnsFailure()
    {
        var result = await Engine().CompensateAsync(Guid.NewGuid());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SendEvent_ExistingInstance_UpdatesContext()
    {
        var engine = Engine();
        var start = await engine.StartWorkflowAsync("order.process");
        var send = await engine.SendEventAsync(start.Value.Id, "payment.confirmed", new { Amount = 100 });
        send.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendEvent_UnknownId_ReturnsFailure()
    {
        var send = await Engine().SendEventAsync(Guid.NewGuid(), "anything");
        send.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task StartMultipleWorkflows_EachGetsUniqueId()
    {
        var engine = Engine();
        var r1 = await engine.StartWorkflowAsync("order.process");
        var r2 = await engine.StartWorkflowAsync("order.process");
        r1.Value.Id.Should().NotBe(r2.Value.Id);
    }
}
