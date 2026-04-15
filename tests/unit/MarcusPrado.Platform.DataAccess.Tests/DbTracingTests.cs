using Dapper;
using FluentAssertions;
using MarcusPrado.Platform.DataAccess.Tracing;
using Microsoft.Data.Sqlite;
using Xunit;

namespace MarcusPrado.Platform.DataAccess.Tests;

public sealed class DbTracingTests
{
    // Test 1: SqlSanitizer replaces string literals with '?'
    [Fact]
    public void SqlSanitizer_Sanitize_ReplacesStringLiterals()
    {
        var sql = "SELECT * FROM users WHERE name = 'John'";
        var result = SqlSanitizer.Sanitize(sql);
        result.Should().Be("SELECT * FROM users WHERE name = ?");
    }

    // Test 2: SqlSanitizer replaces numeric literals with '?'
    [Fact]
    public void SqlSanitizer_Sanitize_ReplacesNumericLiterals()
    {
        var sql = "SELECT * FROM orders WHERE id = 42";
        var result = SqlSanitizer.Sanitize(sql);
        result.Should().Be("SELECT * FROM orders WHERE id = ?");
    }

    // Test 3: SqlSanitizer passes empty/whitespace through unchanged
    [Fact]
    public void SqlSanitizer_Sanitize_EmptyStringPassesThrough()
    {
        SqlSanitizer.Sanitize(string.Empty).Should().Be(string.Empty);
        SqlSanitizer.Sanitize("   ").Should().Be("   ");
    }

    // Test 4: ExtractOperation extracts the first SQL keyword
    [Fact]
    public void EfCoreTracingInterceptor_ExtractOperation_ReturnsFirstKeyword()
    {
        EfCoreTracingInterceptor.ExtractOperation("SELECT * FROM items").Should().Be("SELECT");
        EfCoreTracingInterceptor.ExtractOperation("  INSERT INTO foo VALUES (1)").Should().Be("INSERT");
        EfCoreTracingInterceptor.ExtractOperation("UPDATE foo SET x = 1").Should().Be("UPDATE");
        EfCoreTracingInterceptor.ExtractOperation("DELETE FROM foo WHERE id = 1").Should().Be("DELETE");
    }

    // Test 5: DbActivitySource.Instance.Name matches the expected constant
    [Fact]
    public void DbActivitySource_Name_IsCorrect()
    {
        DbActivitySource.Instance.Name.Should().Be("MarcusPrado.Platform.DataAccess");
        DbActivitySource.Name.Should().Be("MarcusPrado.Platform.DataAccess");
    }

    // Test 6: DapperTracingWrapper.QueryWithTraceAsync executes against SQLite in-memory
    [Fact]
    public async Task DapperTracingWrapper_QueryWithTraceAsync_ReturnsResults()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await conn.ExecuteAsync("CREATE TABLE items (id INTEGER, name TEXT)");
        await conn.ExecuteAsync("INSERT INTO items VALUES (1, 'test')");

        var results = await conn.QueryWithTraceAsync<(long id, string name)>("SELECT id, name FROM items");

        results.Should().ContainSingle(r => r.id == 1 && r.name == "test");
    }

    // Test 7: DapperTracingWrapper.ExecuteWithTraceAsync returns affected row count
    [Fact]
    public async Task DapperTracingWrapper_ExecuteWithTraceAsync_ReturnsAffectedRows()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        await conn.ExecuteAsync("CREATE TABLE items (id INTEGER, name TEXT)");

        var affected = await conn.ExecuteWithTraceAsync("INSERT INTO items VALUES (1, 'hello')");

        affected.Should().Be(1);
    }
}
