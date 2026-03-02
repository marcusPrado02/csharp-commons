using MarcusPrado.Platform.Abstractions.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MarcusPrado.Platform.Application.Tests;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task CallsNextAndReturnsResponse()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<SimpleCommand, Result<string>>>>();
        var behavior = new LoggingBehavior<SimpleCommand, Result<string>>(logger);

        RequestHandlerDelegate<Result<string>> next = _ =>
            Task.FromResult(Result.Success<string>("logged"));

        var result = await behavior.HandleAsync(new SimpleCommand(), next);

        Assert.True(result.IsSuccess);
        Assert.Equal("logged", result.Value);
    }

    [Fact]
    public async Task WhenNextThrows_ExceptionPropagates()
    {
        var logger = Substitute.For<ILogger<LoggingBehavior<SimpleCommand, Result<string>>>>();
        var behavior = new LoggingBehavior<SimpleCommand, Result<string>>(logger);

        RequestHandlerDelegate<Result<string>> next = _ =>
            throw new InvalidOperationException("boom");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.HandleAsync(new SimpleCommand(), next));
    }
}
