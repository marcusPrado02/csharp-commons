using MarcusPrado.Platform.Abstractions.Errors;
using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Abstractions.Tests.Results;

public sealed class ResultTests
{
    // ── Non-generic Result ────────────────────────────────────────────────────

    [Fact]
    public void Success_IsSuccess_IsTrue()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Failure_IsFailure_IsTrue()
    {
        var error = Error.Validation("X.Y", "msg");
        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Failure_Error_ReturnsError()
    {
        var error = Error.Validation("X.Y", "msg");
        var result = Result.Failure(error);

        result.Error.Should().Be(error);
    }

    [Fact]
    public void Success_AccessingError_Throws()
    {
        var result = Result.Success();
        var act = () => result.Error;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_ErrorToResult_ProducesFailure()
    {
        var error = Error.NotFound("X.NOT_FOUND", "not found");
        Result result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Equality_TwoSuccesses_AreEqual()
    {
        Result.Success().Should().Be(Result.Success());
    }

    [Fact]
    public void Equality_SameError_AreEqual()
    {
        var error = Error.Validation("X.Y", "msg");
        Result.Failure(error).Should().Be(Result.Failure(error));
    }

    // ── Generic Result<T> ─────────────────────────────────────────────────────

    [Fact]
    public void SuccessOfT_IsSuccess_IsTrue()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void FailureOfT_IsFailure_IsTrue()
    {
        var error = Error.Validation("X.Y", "msg");
        var result = Result.Failure<int>(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void FailureOfT_AccessingValue_Throws()
    {
        var result = Result.Failure<int>(Error.Validation("X.Y", "msg"));
        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>().WithMessage("*Cannot access*");
    }

    [Fact]
    public void SuccessOfT_AccessingError_Throws()
    {
        var result = Result.Success(99);
        var act = () => result.Error;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_ValueToResultOfT_ProducesSuccess()
    {
        Result<string> result = "hello";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void ImplicitConversion_ErrorToResultOfT_ProducesFailure()
    {
        var error = Error.NotFound("X.NOT_FOUND", "not found");
        Result<string> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_ResultOfT_ToResult_WideningSuccess()
    {
        Result<int> typed = Result.Success(7);
        Result untyped = typed;

        untyped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_ResultOfT_ToResult_WideningFailure()
    {
        var error = Error.Validation("X.Y", "msg");
        Result<int> typed = Result.Failure<int>(error);
        Result untyped = typed;

        untyped.IsFailure.Should().BeTrue();
        untyped.Error.Should().Be(error);
    }

    [Fact]
    public void Deconstruct_Success_ReturnsCorrectTuple()
    {
        Result<int> result = 42;
        var (ok, value, error) = result;

        ok.Should().BeTrue();
        value.Should().Be(42);
        // error slot on success is the internal sentinel — just verify it is not default
        // and that the result is consistent with success
        error.Code.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Deconstruct_Failure_ReturnsCorrectTuple()
    {
        var error = Error.Validation("X.Y", "msg");
        Result<int> result = error;
        var (ok, value, deconstructedError) = result;

        ok.Should().BeFalse();
        value.Should().Be(default(int));
        deconstructedError.Should().Be(error);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        Result.Success("foo").Should().Be(Result.Success("foo"));
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        Result.Success("foo").Should().NotBe(Result.Success("bar"));
    }

    [Fact]
    public void Equality_SameError_AreEqualForTyped()
    {
        var e = Error.Validation("X.Y", "msg");
        Result.Failure<string>(e).Should().Be(Result.Failure<string>(e));
    }
}
