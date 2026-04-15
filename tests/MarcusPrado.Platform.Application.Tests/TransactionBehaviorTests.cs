using MarcusPrado.Platform.Abstractions.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace MarcusPrado.Platform.Application.Tests;

public sealed class TransactionBehaviorTests
{
    [Fact]
    public async Task NoUnitOfWork_CallsNext()
    {
        var logger = NullLogger<TransactionBehavior<SimpleCommand, Result<string>>>.Instance;
        var behavior = new TransactionBehavior<SimpleCommand, Result<string>>(logger);
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
    public async Task NonTransactionalCommand_SkipsUnitOfWork()
    {
        var uow = Substitute.For<IUnitOfWork>();
        var logger = NullLogger<TransactionBehavior<SimpleCommand, Result<string>>>.Instance;
        var behavior = new TransactionBehavior<SimpleCommand, Result<string>>(logger, uow);
        var called = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            called = true;
            return Task.FromResult(Result.Success<string>("not transactional"));
        };

        await behavior.HandleAsync(new SimpleCommand(), next);

        Assert.True(called);
        await uow.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransactionalCommand_Success_CommitsTransaction()
    {
        var uow = Substitute.For<IUnitOfWork>();
        uow.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        uow.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var logger = NullLogger<TransactionBehavior<TransactionalCommand, Result<int>>>.Instance;
        var behavior = new TransactionBehavior<TransactionalCommand, Result<int>>(logger, uow);

        RequestHandlerDelegate<Result<int>> next = _ => Task.FromResult(Result.Success<int>(42));

        var result = await behavior.HandleAsync(new TransactionalCommand(), next);

        Assert.True(result.IsSuccess);
        await uow.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await uow.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransactionalCommand_Failure_RollsBack()
    {
        var uow = Substitute.For<IUnitOfWork>();
        uow.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        uow.RollbackAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var logger = NullLogger<TransactionBehavior<TransactionalCommand, Result<int>>>.Instance;
        var behavior = new TransactionBehavior<TransactionalCommand, Result<int>>(logger, uow);

        RequestHandlerDelegate<Result<int>> next = _ => throw new InvalidOperationException("handler failed");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.HandleAsync(new TransactionalCommand(), next)
        );

        await uow.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        await uow.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}
