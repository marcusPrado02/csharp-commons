namespace MarcusPrado.Platform.EventSourcing.Tests;

// ── Test helpers ──────────────────────────────────────────────────────────────

file sealed record OrderCommand(Guid OrderId, decimal Amount);

// ── Tests ─────────────────────────────────────────────────────────────────────

public sealed class SagaOrchestratorTests
{
    private static SagaOrchestrator CreateOrchestrator() => new();

    // ── 1. Happy path: all steps complete, status → Completed ─────────────────

    [Fact]
    public async Task HappyPath_AllStepsSucceed_StatusIsCompleted()
    {
        var executed = new List<string>();

        var saga = new DefaultSaga<OrderCommand>("saga-1", new OrderCommand(Guid.NewGuid(), 100));
        saga.AddStep(
            new SagaStep<OrderCommand>("Step1", (_, _) => { executed.Add("S1"); return Task.CompletedTask; }),
            saga.State);
        saga.AddStep(
            new SagaStep<OrderCommand>("Step2", (_, _) => { executed.Add("S2"); return Task.CompletedTask; }),
            saga.State);

        await CreateOrchestrator().ExecuteAsync(saga);

        saga.Status.Should().Be(SagaStatus.Completed);
        executed.Should().Equal("S1", "S2");
    }

    // ── 2. On failure, compensations run in reverse order ─────────────────────

    [Fact]
    public async Task OnFailure_CompensationsRunInReverseOrder()
    {
        var log = new List<string>();

        var saga = new DefaultSaga<OrderCommand>("saga-2", new OrderCommand(Guid.NewGuid(), 50));
        saga.AddStep(
            new SagaStep<OrderCommand>(
                "Step1",
                execute: (_, _) => { log.Add("exec-S1"); return Task.CompletedTask; },
                compensate: (_, _) => { log.Add("comp-S1"); return Task.CompletedTask; }),
            saga.State);
        saga.AddStep(
            new SagaStep<OrderCommand>(
                "Step2",
                execute: (_, _) => { log.Add("exec-S2"); return Task.CompletedTask; },
                compensate: (_, _) => { log.Add("comp-S2"); return Task.CompletedTask; }),
            saga.State);
        saga.AddStep(
            new SagaStep<OrderCommand>(
                "Step3",
                execute: (_, _) => throw new InvalidOperationException("Step3 failed")),
            saga.State);

        var act = () => CreateOrchestrator().ExecuteAsync(saga);

        await act.Should().ThrowAsync<SagaExecutionException>();
        log.Should().Equal("exec-S1", "exec-S2", "comp-S2", "comp-S1");
    }

    // ── 3. Status transitions: Running → Compensating → Failed ───────────────

    [Fact]
    public async Task OnFailure_StatusTransitionsTo_Failed()
    {
        var saga = new DefaultSaga<OrderCommand>("saga-3", new OrderCommand(Guid.NewGuid(), 75));
        saga.AddStep(
            new SagaStep<OrderCommand>("Boom", (_, _) => throw new Exception("boom")),
            saga.State);

        try { await CreateOrchestrator().ExecuteAsync(saga); } catch { /* expected */ }

        saga.Status.Should().Be(SagaStatus.Failed);
    }

    // ── 4. SagaExecutionException carries the correct FailedStepName ──────────

    [Fact]
    public async Task OnFailure_ExceptionContains_FailedStepName()
    {
        const string stepName = "PaymentStep";
        var saga = new DefaultSaga<OrderCommand>("saga-4", new OrderCommand(Guid.NewGuid(), 200));
        saga.AddStep(
            new SagaStep<OrderCommand>(stepName, (_, _) => throw new InvalidOperationException("card declined")),
            saga.State);

        var ex = await Assert.ThrowsAsync<SagaExecutionException>(
            () => CreateOrchestrator().ExecuteAsync(saga));

        ex.FailedStepName.Should().Be(stepName);
        ex.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    // ── 5. Step without compensation: no compensation call on failure ─────────

    [Fact]
    public async Task Step_WithoutCompensation_DoesNotThrow_DuringRollback()
    {
        var log = new List<string>();
        var saga = new DefaultSaga<OrderCommand>("saga-5", new OrderCommand(Guid.NewGuid(), 10));

        // Step 1 has NO compensation
        saga.AddStep(
            new SagaStep<OrderCommand>("NoComp", (_, _) => { log.Add("S1"); return Task.CompletedTask; }),
            saga.State);
        saga.AddStep(
            new SagaStep<OrderCommand>("Fail", (_, _) => throw new Exception("fail")),
            saga.State);

        var act = () => CreateOrchestrator().ExecuteAsync(saga);

        await act.Should().ThrowAsync<SagaExecutionException>();
        log.Should().ContainSingle("S1"); // executed but not compensated — no crash
        saga.Status.Should().Be(SagaStatus.Failed);
    }

    // ── 6. Timeout: step exceeding its timeout throws OperationCanceledException

    [Fact]
    public async Task Step_WithTimeout_ThrowsWhenExceeded()
    {
        var saga = new DefaultSaga<OrderCommand>("saga-6", new OrderCommand(Guid.NewGuid(), 300));
        saga.AddStep(
            new SagaStep<OrderCommand>(
                "SlowStep",
                execute: async (_, ct) => await Task.Delay(TimeSpan.FromSeconds(10), ct),
                timeout: TimeSpan.FromMilliseconds(50)),
            saga.State);

        var ex = await Assert.ThrowsAsync<SagaExecutionException>(
            () => CreateOrchestrator().ExecuteAsync(saga));

        ex.InnerException.Should().BeAssignableTo<OperationCanceledException>();
        saga.Status.Should().Be(SagaStatus.Failed);
    }

    // ── 7. Empty saga: no steps → immediately Completed ───────────────────────

    [Fact]
    public async Task EmptySaga_CompletesImmediately()
    {
        var saga = new DefaultSaga<OrderCommand>("saga-7", new OrderCommand(Guid.NewGuid(), 0));

        await CreateOrchestrator().ExecuteAsync(saga);

        saga.Status.Should().Be(SagaStatus.Completed);
    }

    // ── 8. SagaCompensationHandler: compensates in LIFO order ─────────────────

    [Fact]
    public async Task CompensationHandler_ExecutesInReverseRegistrationOrder()
    {
        var log = new List<int>();
        var handler = new SagaCompensationHandler();

        handler.Register(_ => { log.Add(1); return Task.CompletedTask; });
        handler.Register(_ => { log.Add(2); return Task.CompletedTask; });
        handler.Register(_ => { log.Add(3); return Task.CompletedTask; });

        await handler.CompensateAsync();

        log.Should().Equal(3, 2, 1);
    }

    // ── 9. InMemorySagaStore: save and load roundtrip ─────────────────────────

    [Fact]
    public async Task InMemorySagaStore_SaveAndLoad_Roundtrip()
    {
        var store = new InMemorySagaStore<OrderCommand>();
        var saga = new DefaultSaga<OrderCommand>("saga-store-1", new OrderCommand(Guid.NewGuid(), 99));

        await store.SaveAsync(saga);

        var loaded = await store.LoadAsync("saga-store-1");

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be("saga-store-1");
        loaded.State.Amount.Should().Be(99);
    }

    // ── 10. InMemorySagaStore: missing id returns null ────────────────────────

    [Fact]
    public async Task InMemorySagaStore_LoadMissing_ReturnsNull()
    {
        var store = new InMemorySagaStore<OrderCommand>();

        var result = await store.LoadAsync("does-not-exist");

        result.Should().BeNull();
    }

    // ── 11. InMemorySagaStore: save updates existing saga ─────────────────────

    [Fact]
    public async Task InMemorySagaStore_SaveTwice_UpdatesExisting()
    {
        var store = new InMemorySagaStore<OrderCommand>();
        var saga = new DefaultSaga<OrderCommand>("saga-store-2", new OrderCommand(Guid.NewGuid(), 10));

        await store.SaveAsync(saga);
        saga.Status = SagaStatus.Completed;
        await store.SaveAsync(saga);

        store.Count.Should().Be(1); // not duplicated
        var loaded = await store.LoadAsync("saga-store-2");
        loaded!.Status.Should().Be(SagaStatus.Completed);
    }

    // ── 12. BoundSagaStep: HasCompensation reflects presence of delegate ──────

    [Fact]
    public void BoundSagaStep_HasCompensation_ReflectsDelegate()
    {
        var withComp = new BoundSagaStep<OrderCommand>(
            new SagaStep<OrderCommand>(
                "WithComp",
                (_, _) => Task.CompletedTask,
                (_, _) => Task.CompletedTask),
            new OrderCommand(Guid.NewGuid(), 1));

        var noComp = new BoundSagaStep<OrderCommand>(
            new SagaStep<OrderCommand>("NoComp", (_, _) => Task.CompletedTask),
            new OrderCommand(Guid.NewGuid(), 1));

        withComp.HasCompensation.Should().BeTrue();
        noComp.HasCompensation.Should().BeFalse();
    }

    // ── 13. Orchestrator: status is Running while first step executes ─────────

    [Fact]
    public async Task Orchestrator_StatusIsRunning_BeforeCompletion()
    {
        SagaStatus? capturedStatus = null;
        var saga = new DefaultSaga<OrderCommand>("saga-13", new OrderCommand(Guid.NewGuid(), 5));
        saga.AddStep(
            new SagaStep<OrderCommand>("Capture", (_, _) =>
            {
                // capture status at execution time
                capturedStatus = saga.Status;
                return Task.CompletedTask;
            }),
            saga.State);

        await CreateOrchestrator().ExecuteAsync(saga);

        capturedStatus.Should().Be(SagaStatus.Running);
        saga.Status.Should().Be(SagaStatus.Completed);
    }
}
