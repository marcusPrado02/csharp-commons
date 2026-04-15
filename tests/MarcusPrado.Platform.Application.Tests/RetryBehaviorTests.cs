using MarcusPrado.Platform.Abstractions.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace MarcusPrado.Platform.Application.Tests;

public sealed class RetryBehaviorTests
{
    [Fact]
    public async Task NonTransientException_DoesNotRetry()
    {
        var logger = NullLogger<RetryBehavior<SimpleCommand, Result<string>>>.Instance;
        var behavior = new RetryBehavior<SimpleCommand, Result<string>>(logger);
        var calls = 0;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            calls++;
            throw new InvalidOperationException("not transient");
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.HandleAsync(new SimpleCommand(), next, CancellationToken.None)
        );

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Success_ReturnsOnFirstCall()
    {
        var logger = NullLogger<RetryBehavior<SimpleCommand, Result<string>>>.Instance;
        var behavior = new RetryBehavior<SimpleCommand, Result<string>>(logger);
        var calls = 0;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            calls++;
            return Task.FromResult(Result.Success<string>("ok"));
        };

        var result = await behavior.HandleAsync(new SimpleCommand(), next);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, calls);
    }
}
