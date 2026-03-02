using MarcusPrado.Platform.Abstractions.Results;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Application.Tests;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task NoValidators_CallsNext()
    {
        var sp = new ServiceCollection().BuildServiceProvider();
        var behavior = new ValidationBehavior<SimpleCommand, Result<string>>(sp);
        var called = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result.Success<string>("ok"));
        };

        var result = await behavior.HandleAsync(new SimpleCommand(), next);

        Assert.True(called);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidValidator_CallsNext()
    {
        var validator = Substitute.For<IValidator<SimpleCommand>>();
        validator.ValidateAsync(Arg.Any<SimpleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IValidationResult>(new TestValidationResult(true)));

        var sp = new ServiceCollection()
            .AddSingleton(validator)
            .BuildServiceProvider();

        var behavior = new ValidationBehavior<SimpleCommand, Result<string>>(sp);
        var called = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result.Success<string>("done"));
        };

        var result = await behavior.HandleAsync(new SimpleCommand(), next);

        Assert.True(called);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task FailingValidator_ReturnsFailureWithoutCallingNext()
    {
        var validationError = Error.Validation("CMD.INVALID", "Bad data");
        var validator = Substitute.For<IValidator<SimpleCommand>>();
        validator.ValidateAsync(Arg.Any<SimpleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IValidationResult>(
                new TestValidationResult(false, [validationError])));

        var sp = new ServiceCollection()
            .AddSingleton(validator)
            .BuildServiceProvider();

        var behavior = new ValidationBehavior<SimpleCommand, Result<string>>(sp);
        var called = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result.Success<string>("should not reach here"));
        };

        var result = await behavior.HandleAsync(new SimpleCommand(), next);

        Assert.False(called);
        Assert.True(result.IsFailure);
        Assert.Equal(validationError.Code, result.Error.Code);
    }

    [Fact]
    public async Task MultipleValidators_AggregatesErrors_ReturnsFistError()
    {
        var err1 = Error.Validation("CMD.ERR1", "Error 1");
        var err2 = Error.Validation("CMD.ERR2", "Error 2");

        var v1 = Substitute.For<IValidator<SimpleCommand>>();
        v1.ValidateAsync(Arg.Any<SimpleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IValidationResult>(new TestValidationResult(false, [err1])));

        var v2 = Substitute.For<IValidator<SimpleCommand>>();
        v2.ValidateAsync(Arg.Any<SimpleCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IValidationResult>(new TestValidationResult(false, [err2])));

        var sp = new ServiceCollection()
            .AddSingleton(v1)
            .AddSingleton(v2)
            .BuildServiceProvider();

        var behavior = new ValidationBehavior<SimpleCommand, Result<string>>(sp);
        var result = await behavior.HandleAsync(new SimpleCommand(), _ => Task.FromResult(Result.Success<string>("x")));

        Assert.True(result.IsFailure);
    }
}
