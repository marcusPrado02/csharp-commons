using MarcusPrado.Platform.Abstractions.Errors;
using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Abstractions.Tests.Results;

public sealed class ResultExtensionsTests
{
    private static readonly Error SomeError = Error.Validation("X.INVALID", "invalid");

    // ── Map ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        Result<int> result = 3;
        var mapped = result.Map(x => x * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(6);
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        Result<int> result = SomeError;
        var mapped = result.Map(x => x * 2);

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(SomeError);
    }

    [Fact]
    public void Map_NullMapper_Throws()
    {
        Result<int> result = 1;
        var act = () => result.Map<int, string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Bind ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Bind_OnSuccess_ChainsNextResult()
    {
        Result<int> result = 5;
        var bound = result.Bind(x => x > 0 ? Result.Success(x.ToString()) : SomeError);

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_OnSuccess_NextFails_ReturnsThatFailure()
    {
        Result<int> result = -1;
        var innerError = Error.Validation("X.NEG", "must be positive");
        var bound = result.Bind<int, string>(_ => innerError);

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be(innerError);
    }

    [Fact]
    public void Bind_OnFailure_SkipsFunction()
    {
        Result<int> result = SomeError;
        var invoked = false;
        result.Bind(x =>
        {
            invoked = true;
            return Result.Success(x);
        });

        invoked.Should().BeFalse();
    }

    // ── Match ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Match_OnSuccess_CallsOnSuccess()
    {
        Result<int> result = 7;
        var output = result.Match(v => $"ok:{v}", e => $"err:{e.Code}");

        output.Should().Be("ok:7");
    }

    [Fact]
    public void Match_OnFailure_CallsOnFailure()
    {
        Result<int> result = SomeError;
        var output = result.Match(v => $"ok:{v}", e => $"err:{e.Code}");

        output.Should().Be("err:X.INVALID");
    }

    // ── OnSuccess / OnFailure ─────────────────────────────────────────────────

    [Fact]
    public void OnSuccess_OnSuccess_Invokes_ReturnsOriginal()
    {
        Result<int> result = 10;
        var invoked = 0;
        var passThrough = result.OnSuccess(v => invoked = v);

        invoked.Should().Be(10);
        passThrough.Should().Be(result);
    }

    [Fact]
    public void OnSuccess_OnFailure_DoesNotInvoke()
    {
        Result<int> result = SomeError;
        var invoked = false;
        result.OnSuccess(_ => invoked = true);

        invoked.Should().BeFalse();
    }

    [Fact]
    public void OnFailure_OnFailure_Invokes_ReturnsOriginal()
    {
        Result<int> result = SomeError;
        Error? captured = null;
        var passThrough = result.OnFailure(e => captured = e);

        captured.Should().Be(SomeError);
        passThrough.Should().Be(result);
    }

    [Fact]
    public void OnFailure_OnSuccess_DoesNotInvoke()
    {
        Result<int> result = 1;
        var invoked = false;
        result.OnFailure(_ => invoked = true);

        invoked.Should().BeFalse();
    }

    // ── Ensure ────────────────────────────────────────────────────────────────

    [Fact]
    public void Ensure_PredicateTrue_KeepsSuccess()
    {
        Result<int> result = 5;
        var ensured = result.Ensure(v => v > 0, SomeError);

        ensured.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Ensure_PredicateFalse_FailsWithGivenError()
    {
        Result<int> result = -1;
        var ensured = result.Ensure(v => v > 0, SomeError);

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be(SomeError);
    }

    [Fact]
    public void Ensure_AlreadyFailed_PassesThrough()
    {
        Result<int> result = SomeError;
        var otherError = Error.Technical("Y.ERR", "other");
        var ensured = result.Ensure(_ => true, otherError);

        ensured.Error.Should().Be(SomeError); // original error preserved
    }

    // ── MapError ──────────────────────────────────────────────────────────────

    [Fact]
    public void MapError_OnFailure_TransformsError()
    {
        Result<int> result = SomeError;
        var newError = Error.Technical("Y.ERR", "technical");
        var mapped = result.MapError(_ => newError);

        mapped.Error.Should().Be(newError);
    }

    [Fact]
    public void MapError_OnSuccess_IgnoresFunction()
    {
        Result<int> result = 5;
        var invoked = false;
        var mapped = result.MapError(e =>
        {
            invoked = true;
            return e;
        });

        invoked.Should().BeFalse();
        mapped.IsSuccess.Should().BeTrue();
    }

    // ── ToResult ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToResult_OnSuccess_ReturnsSuccessResult()
    {
        Result<int> typed = 9;
        Result untyped = typed.ToResult();

        untyped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToResult_OnFailure_ReturnsFailureResult()
    {
        Result<int> typed = SomeError;
        Result untyped = typed.ToResult();

        untyped.IsFailure.Should().BeTrue();
        untyped.Error.Should().Be(SomeError);
    }

    // ── GetValueOrDefault / GetValueOrElse ────────────────────────────────────

    [Fact]
    public void GetValueOrDefault_OnSuccess_ReturnsValue()
    {
        Result<int> result = 42;
        result.GetValueOrDefault().Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ReturnsDefault()
    {
        Result<int> result = SomeError;
        result.GetValueOrDefault().Should().Be(0);
    }

    [Fact]
    public void GetValueOrElse_OnFailure_ReturnsFallback()
    {
        Result<int> result = SomeError;
        result.GetValueOrElse(_ => 99).Should().Be(99);
    }

    [Fact]
    public void GetValueOrElse_OnSuccess_SkipsFallback()
    {
        Result<int> result = 7;
        var invoked = false;
        var value = result.GetValueOrElse(_ =>
        {
            invoked = true;
            return 0;
        });

        invoked.Should().BeFalse();
        value.Should().Be(7);
    }

    // ── Combine ───────────────────────────────────────────────────────────────

    [Fact]
    public void Combine_AllSuccess_ReturnsSuccessWithAllValues()
    {
        var results = new[] { Result.Success(1), Result.Success(2), Result.Success(3) };
        var combined = results.Combine();

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Combine_FirstFailure_ShortCircuits()
    {
        var err = Error.Validation("X.FAIL", "fail");
        var results = new[] { Result.Success(1), Result.Failure<int>(err), Result.Success(3) };
        var combined = results.Combine();

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be(err);
    }

    // ── CombineAll ────────────────────────────────────────────────────────────

    [Fact]
    public void CombineAll_AllSuccess_ReturnsSuccessWithAllValues()
    {
        var results = new[] { Result.Success(10), Result.Success(20) };
        var combined = results.CombineAll();

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Equal(10, 20);
    }

    [Fact]
    public void CombineAll_MultipleFailures_CollectsAllErrors()
    {
        var err1 = Error.Validation("X.A", "a");
        var err2 = Error.Validation("X.B", "b");
        var results = new[] { Result.Success(1), Result.Failure<int>(err1), Result.Failure<int>(err2) };

        var combined = results.CombineAll();

        combined.IsFailure.Should().BeTrue();
        combined.Error.Code.Should().Be("VALIDATION.MULTIPLE_ERRORS");
        combined.Error.Metadata.Should().ContainKey("errors[0].code").WhoseValue.Should().Be("X.A");
        combined.Error.Metadata.Should().ContainKey("errors[1].code").WhoseValue.Should().Be("X.B");
    }
}
