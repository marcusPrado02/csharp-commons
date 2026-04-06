// <copyright file="CliTests.cs" company="MarcusPrado">
// Copyright (c) MarcusPrado. All rights reserved.
// </copyright>

using FluentAssertions;
using MarcusPrado.Platform.Cli.Commands;
using Xunit;

namespace MarcusPrado.Platform.Cli.Tests;

public sealed class ScaffoldCommandTests
{
    [Fact]
    public void Execute_NoArgs_ReturnsOne()
    {
        var result = ScaffoldCommand.Execute([]);
        result.Should().Be(1);
    }

    [Fact]
    public void Execute_ValidTemplate_ReturnsZero()
    {
        var result = ScaffoldCommand.Execute(["api", "MyApp"]);
        result.Should().Be(0);
    }

    [Fact]
    public void Execute_OnlyTemplateNoName_ReturnsZero()
    {
        var result = ScaffoldCommand.Execute(["worker"]);
        result.Should().Be(0);
    }
}

public sealed class ConfigCommandTests
{
    [Fact]
    public void Execute_NoArgs_ReturnsOne()
    {
        var result = ConfigCommand.Execute([]);
        result.Should().Be(1);
    }

    [Fact]
    public void Execute_WrongSubCommand_ReturnsOne()
    {
        var result = ConfigCommand.Execute(["decrypt", "something"]);
        result.Should().Be(1);
    }

    [Fact]
    public void Execute_EncryptValue_ReturnsZero()
    {
        var result = ConfigCommand.Execute(["encrypt", "my-secret"]);
        result.Should().Be(0);
    }
}

public sealed class CatalogCommandTests
{
    [Fact]
    public void Execute_NoArgs_ReturnsOne()
    {
        var result = CatalogCommand.Execute([]);
        result.Should().Be(1);
    }

    [Fact]
    public void Execute_WrongSubCommand_ReturnsOne()
    {
        var result = CatalogCommand.Execute(["codes"]);
        result.Should().Be(1);
    }

    [Fact]
    public void Execute_Errors_ReturnsZero()
    {
        var result = CatalogCommand.Execute(["errors"]);
        result.Should().Be(0);
    }
}

public sealed class ArchCommandTests
{
    [Fact]
    public void Execute_NoArgs_ReturnsOne()
    {
        var result = ArchCommand.Execute([]);
        result.Should().Be(1);
    }

    [Fact]
    public void Execute_WrongSubCommand_ReturnsOne()
    {
        var result = ArchCommand.Execute(["check"]);
        result.Should().Be(1);
    }

    [Fact]
    public void Execute_Validate_ReturnsZero()
    {
        var result = ArchCommand.Execute(["validate"]);
        result.Should().Be(0);
    }
}

public sealed class DlqCommandTests
{
    [Fact]
    public void Execute_NoArgs_ReturnsOne()
    {
        var result = DlqCommand.Execute([]);
        result.Should().Be(1);
    }

    [Fact]
    public void Execute_WrongSubCommand_ReturnsOne()
    {
        var result = DlqCommand.Execute(["list"]);
        result.Should().Be(1);
    }

    [Fact]
    public void Execute_InspectWithUrl_ReturnsZero()
    {
        var result = DlqCommand.Execute(["inspect", "amqp://localhost/my-dlq"]);
        result.Should().Be(0);
    }
}

public sealed class HealthCommandTests
{
    [Fact]
    public async Task ExecuteAsync_NoArgs_ReturnsOne()
    {
        var result = await HealthCommand.ExecuteAsync([]);
        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidHost_ReturnsOne()
    {
        // Use a non-routable IP so it fails fast with a connection error.
        var result = await HealthCommand.ExecuteAsync(["http://192.0.2.1"]);
        result.Should().Be(1);
    }
}
