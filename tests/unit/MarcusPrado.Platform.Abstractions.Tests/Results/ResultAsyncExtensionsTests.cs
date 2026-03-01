using MarcusPrado.Platform.Abstractions.Errors;
using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Abstractions.Tests.Results;

public sealed class ResultAsyncExtensionsTests
{
    private static readonly Error SomeError = Error.Validation("X.ASYNC", "async error");

    private static Task<Result<T>> Ok<T>(T value) => Task.FromResult(Result.Success(value));

    private static Task<Result<T>> Fail<T>(Error e) => Task.FromResult(Result.Failure<T>(e));

    // ── MapAsync (sync mapper) ────────────────────────────────────────────────

    [Fact]
    public async Task MapAsync_SyncMapper_OnSuccess_TransformsValue()
    {
        var result = await Ok(3).MapAsync(x => x * 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(30);
    }

    [Fact]
    public async Task MapAsync_SyncMapper_OnFailure_PropagatesError()
    {
        var result = await Fail<int>(SomeError).MapAsync(x => x * 10);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SomeError);
    }

    // ── MapAsync (async mapper) ───────────────────────────────────────────────

    [Fact]
    public async Task MapAsync_AsyncMapper_OnSuccess_TransformsValue()
    {
        var result = await Ok(3).MapAsync(x => Task.FromResult(x * 10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(30);
    }

    [Fact]
    public async Task MapAsync_AsyncMapper_OnFailure_PropagatesError()
    {
        var result = await Fail<int>(SomeError).MapAsync(x => Task.FromResult(x * 10));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SomeError);
    }

    // ── BindAsync (async binder) ──────────────────────────────────────────────

    [Fact]
    public async Task BindAsync_OnSuccess_ChainsNextResult()
    {
        var result = await Ok(5).BindAsync(x => Task.FromResult<Result<string>>(x.ToString()));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("5");
    }

    [Fact]
    public async Task BindAsync_OnSuccess_NextFails_ReturnsThatFailure()
    {
        var innerErr = Error.Validation("X.NEG", "negative");
        var result = await Ok(-1).BindAsync(_ => Task.FromResult<Result<string>>(innerErr));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(innerErr);
    }

    [Fact]
    public async Task BindAsync_OnFailure_SkipsFunction()
    {
        var invoked = false;
        var result = await Fail<int>(SomeError)
            .BindAsync(x =>
            {
                invoked = true;
                return Task.FromResult<Result<string>>(x.ToString());
            });

        invoked.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    // ── BindAsync (sync binder on async task) ─────────────────────────────────

    [Fact]
    public async Task BindAsync_SyncBinder_OnSuccess_ChainsNextResult()
    {
        var result = await Ok(4).BindAsync((int x) => Result.Success(x * 2));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(8);
    }

    // ── BindAsync → non-generic Result ───────────────────────────────────────

    [Fact]
    public async Task BindAsync_ToNonGeneric_OnSuccess_ReturnsSuccessResult()
    {
        var result = await Ok(1).BindAsync(_ => Task.FromResult(Result.Success()));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_ToNonGeneric_OnFailure_PropagatesError()
    {
        var result = await Fail<int>(SomeError).BindAsync(_ => Task.FromResult(Result.Success()));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SomeError);
    }

    // ── MatchAsync (sync handlers) ────────────────────────────────────────────

    [Fact]
    public async Task MatchAsync_SyncHandlers_OnSuccess_CallsOnSuccess()
    {
        var output = await Ok(7).MatchAsync(v => $"ok:{v}", e => $"err:{e.Code}");

        output.Should().Be("ok:7");
    }

    [Fact]
    public async Task MatchAsync_SyncHandlers_OnFailure_CallsOnFailure()
    {
        var output = await Fail<int>(SomeError).MatchAsync(v => $"ok:{v}", e => $"err:{e.Code}");

        output.Should().Be("err:X.ASYNC");
    }

    // ── MatchAsync (async handlers) ───────────────────────────────────────────

    [Fact]
    public async Task MatchAsync_AsyncHandlers_OnSuccess_CallsOnSuccess()
    {
        var output = await Ok(7).MatchAsync(v => Task.FromResult($"ok:{v}"), e => Task.FromResult($"err:{e.Code}"));

        output.Should().Be("ok:7");
    }

    [Fact]
    public async Task MatchAsync_AsyncHandlers_OnFailure_CallsOnFailure()
    {
        var output = await Fail<int>(SomeError)
            .MatchAsync(v => Task.FromResult($"ok:{v}"), e => Task.FromResult($"err:{e.Code}"));

        output.Should().Be("err:X.ASYNC");
    }

    // ── OnSuccessAsync / OnFailureAsync ───────────────────────────────────────

    [Fact]
    public async Task OnSuccessAsync_OnSuccess_InvokesAction_ReturnsOriginal()
    {
        var captured = 0;
        var result = await Ok(42)
            .OnSuccessAsync(v =>
            {
                captured = v;
                return Task.CompletedTask;
            });

        captured.Should().Be(42);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task OnSuccessAsync_OnFailure_DoesNotInvoke()
    {
        var invoked = false;
        await Fail<int>(SomeError)
            .OnSuccessAsync(_ =>
            {
                invoked = true;
                return Task.CompletedTask;
            });

        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task OnFailureAsync_OnFailure_InvokesAction_ReturnsOriginal()
    {
        Error? captured = null;
        var result = await Fail<int>(SomeError)
            .OnFailureAsync(e =>
            {
                captured = e;
                return Task.CompletedTask;
            });

        captured.Should().Be(SomeError);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task OnFailureAsync_OnSuccess_DoesNotInvoke()
    {
        var invoked = false;
        await Ok(1)
            .OnFailureAsync(_ =>
            {
                invoked = true;
                return Task.CompletedTask;
            });

        invoked.Should().BeFalse();
    }

    // ── EnsureAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task EnsureAsync_PredicateTrue_KeepsSuccess()
    {
        var result = await Ok(5).EnsureAsync(v => Task.FromResult(v > 0), SomeError);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EnsureAsync_PredicateFalse_FailsWithGivenError()
    {
        var result = await Ok(-1).EnsureAsync(v => Task.FromResult(v > 0), SomeError);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SomeError);
    }

    [Fact]
    public async Task EnsureAsync_AlreadyFailed_PassesThroughOriginalError()
    {
        var otherErr = Error.Technical("Y.ERR", "other");
        var result = await Fail<int>(SomeError).EnsureAsync(_ => Task.FromResult(true), otherErr);

        result.Error.Should().Be(SomeError);
    }
}
