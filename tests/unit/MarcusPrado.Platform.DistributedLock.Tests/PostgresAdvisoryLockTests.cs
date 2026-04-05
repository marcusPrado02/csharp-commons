using System.Data;

namespace MarcusPrado.Platform.DistributedLock.Tests;

public sealed class PostgresAdvisoryLockTests
{
    private static IDbConnection MakeConnection(object? scalarResult)
    {
        var param = Substitute.For<IDbDataParameter>();
        param.ParameterName = string.Empty;

        var paramCollection = Substitute.For<IDataParameterCollection>();

        var cmd = Substitute.For<IDbCommand>();
        cmd.CreateParameter().Returns(param);
        cmd.Parameters.Returns(paramCollection);
        cmd.ExecuteScalar().Returns(scalarResult);

        var conn = Substitute.For<IDbConnection>();
        conn.State.Returns(ConnectionState.Open);
        conn.CreateCommand().Returns(cmd);

        return conn;
    }

    [Fact]
    public async Task AcquireAsync_WhenPostgresReturnsTrue_ReturnsNonNullHandle()
    {
        var conn = MakeConnection(true);
        var sut = new PostgresAdvisoryLock(conn);

        var handle = await sut.AcquireAsync("pg-key", TimeSpan.FromSeconds(30));

        handle.Should().NotBeNull();
    }

    [Fact]
    public async Task AcquireAsync_WhenPostgresReturnsFalse_ReturnsNull()
    {
        var conn = MakeConnection(false);
        var sut = new PostgresAdvisoryLock(conn);

        var handle = await sut.AcquireAsync("pg-key", TimeSpan.FromSeconds(30));

        handle.Should().BeNull();
    }

    [Fact]
    public async Task AcquireAsync_WhenPostgresReturnsStringTrue_ReturnsNonNullHandle()
    {
        var conn = MakeConnection("True");
        var sut = new PostgresAdvisoryLock(conn);

        var handle = await sut.AcquireAsync("pg-key", TimeSpan.FromSeconds(30));

        handle.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_DisposeAsync_IsNoOp()
    {
        var conn = MakeConnection(true);
        var sut = new PostgresAdvisoryLock(conn);

        var handle = await sut.AcquireAsync("pg-key", TimeSpan.FromSeconds(30));

        // Should complete without throwing — lock is released by the transaction
        var act = () => handle!.DisposeAsync().AsTask();
        await act.Should().NotThrowAsync();
    }
}
