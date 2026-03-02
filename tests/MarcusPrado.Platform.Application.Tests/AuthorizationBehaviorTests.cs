using MarcusPrado.Platform.Abstractions.Results;

namespace MarcusPrado.Platform.Application.Tests;

public sealed class AuthorizationBehaviorTests
{
    [Fact]
    public async Task DefaultBehavior_PassesThrough()
    {
        var behavior = new AuthorizationBehavior<SimpleCommand, Result<string>>();
        var called = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result.Success<string>("authorized"));
        };

        var result = await behavior.HandleAsync(new SimpleCommand(), next);

        Assert.True(called);
        Assert.True(result.IsSuccess);
    }
}
