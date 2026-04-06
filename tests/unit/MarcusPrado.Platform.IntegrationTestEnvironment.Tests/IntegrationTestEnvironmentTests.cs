using FluentAssertions;
using MarcusPrado.Platform.IntegrationTestEnvironment;
using Xunit;

namespace MarcusPrado.Platform.IntegrationTestEnvironment.Tests;

/// <summary>
/// Unit tests for IntegrationTestEnvironment — no real Docker containers are started.
/// </summary>
public sealed class IntegrationTestEnvironmentTests
{
    // ── PlatformTestEnvironmentBuilder ────────────────────────────────────────

    /// <summary>
    /// A builder with no containers configured should produce an environment object
    /// without throwing.
    /// </summary>
    [Fact]
    public void PlatformTestEnvironmentBuilder_NoContainersConfigured_BuildsEmptyEnvironment()
    {
        // Arrange / Act
        var env = PlatformTestEnvironment.CreateBuilder().Build();

        // Assert
        env.Should().NotBeNull();
    }

    // ── SnapshotRestorer — RegisterTable ──────────────────────────────────────

    /// <summary>
    /// Registering a table name should be reflected in the truncate script.
    /// </summary>
    [Fact]
    public void SnapshotRestorer_RegisterTable_AddsTable()
    {
        // Arrange
        var restorer = new SnapshotRestorer();

        // Act
        restorer.RegisterTable("orders");

        // Assert
        restorer.GetTruncateScript().Should().Contain("orders");
    }

    // ── SnapshotRestorer — GetTruncateScript (single table) ───────────────────

    /// <summary>
    /// A single registered table should appear in the truncate script with the
    /// correct RESTART IDENTITY CASCADE suffix.
    /// </summary>
    [Fact]
    public void SnapshotRestorer_GetTruncateScript_ContainsTableName()
    {
        // Arrange
        var restorer = new SnapshotRestorer();
        restorer.RegisterTable("customers");

        // Act
        var script = restorer.GetTruncateScript();

        // Assert
        script.Should().StartWith("TRUNCATE TABLE");
        script.Should().Contain("customers");
        script.Should().Contain("RESTART IDENTITY CASCADE");
    }

    // ── SnapshotRestorer — GetTruncateScript (multiple tables) ────────────────

    /// <summary>
    /// Multiple registered tables should be separated by commas in the script.
    /// </summary>
    [Fact]
    public void SnapshotRestorer_GetTruncateScript_MultipleTablesWithCommas()
    {
        // Arrange
        var restorer = new SnapshotRestorer();
        restorer.RegisterTable("orders");
        restorer.RegisterTable("order_items");
        restorer.RegisterTable("customers");

        // Act
        var script = restorer.GetTruncateScript();

        // Assert
        script.Should().Contain("orders");
        script.Should().Contain("order_items");
        script.Should().Contain("customers");
        // All three separated by commas (at least two commas for three tables)
        script.Count(c => c == ',').Should().Be(2);
    }

    // ── SnapshotRestorer — Clear ──────────────────────────────────────────────

    /// <summary>
    /// Calling Clear should remove all registered tables so the script becomes empty.
    /// </summary>
    [Fact]
    public void SnapshotRestorer_Clear_RemovesAllTables()
    {
        // Arrange
        var restorer = new SnapshotRestorer();
        restorer.RegisterTable("payments");

        // Act
        restorer.Clear();

        // Assert
        restorer.GetTruncateScript().Should().BeEmpty();
    }

    // ── SnapshotRestorer — GetTruncateScript (no tables) ─────────────────────

    /// <summary>
    /// When no tables are registered, GetTruncateScript should return an empty string.
    /// </summary>
    [Fact]
    public void SnapshotRestorer_GetTruncateScript_NoTables_ReturnsEmpty()
    {
        // Arrange
        var restorer = new SnapshotRestorer();

        // Act
        var script = restorer.GetTruncateScript();

        // Assert
        script.Should().BeEmpty();
    }
}
