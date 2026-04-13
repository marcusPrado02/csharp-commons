using MarcusPrado.Platform.Abstractions.Errors;

namespace MarcusPrado.Platform.Abstractions.Tests.Errors;

public sealed class ErrorTests
{
    // ── Factory: Validation ───────────────────────────────────────────────────

    [Fact]
    public void Validation_SetsCodeMessageAndCategory()
    {
        var error = Error.Validation("USER.INVALID_EMAIL", "Email is not valid.");

        error.Code.Should().Be("USER.INVALID_EMAIL");
        error.Message.Should().Be("Email is not valid.");
        error.Category.Should().Be(ErrorCategory.Validation);
        error.Severity.Should().Be(ErrorSeverity.Warning);
        error.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Validation_WithFieldAndAttemptedValue_PopulatesMetadata()
    {
        var error = Error.Validation("USER.INVALID_EMAIL", "Email is not valid.", "email", "bad@@email");

        error.Metadata.Should().ContainKey("field").WhoseValue.Should().Be("email");
        error.Metadata.Should().ContainKey("attemptedValue").WhoseValue.Should().Be("bad@@email");
    }

    // ── Factory: all categories ───────────────────────────────────────────────

    [Theory]
    [InlineData(nameof(ErrorCategory.NotFound))]
    [InlineData(nameof(ErrorCategory.Conflict))]
    [InlineData(nameof(ErrorCategory.Unauthorized))]
    [InlineData(nameof(ErrorCategory.Forbidden))]
    [InlineData(nameof(ErrorCategory.Technical))]
    [InlineData(nameof(ErrorCategory.External))]
    [InlineData(nameof(ErrorCategory.Timeout))]
    [InlineData(nameof(ErrorCategory.Unavailable))]
    public void FactoryMethod_SetsCorrectCategory(string categoryName)
    {
        var category = Enum.Parse<ErrorCategory>(categoryName);
        var code = $"SVC.{categoryName.ToUpperInvariant()}";
        const string message = "test message";

        var error = category switch
        {
            ErrorCategory.NotFound => Error.NotFound(code, message),
            ErrorCategory.Conflict => Error.Conflict(code, message),
            ErrorCategory.Unauthorized => Error.Unauthorized(code, message),
            ErrorCategory.Forbidden => Error.Forbidden(code, message),
            ErrorCategory.Technical => Error.Technical(code, message),
            ErrorCategory.External => Error.External(code, message),
            ErrorCategory.Timeout => Error.Timeout(code, message),
            ErrorCategory.Unavailable => Error.Unavailable(code, message),
            _ => throw new InvalidOperationException(),
        };

        error.Category.Should().Be(category);
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
    }

    // ── Constructor guards ────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrWhitespaceCode_Throws(string? code)
    {
        var act = () => Error.Validation(code!, "msg");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrWhitespaceMessage_Throws(string? message)
    {
        var act = () => Error.Validation("CODE", message!);
        act.Should().Throw<ArgumentException>();
    }

    // ── WithMetadata ──────────────────────────────────────────────────────────

    [Fact]
    public void WithMetadata_AddsKeyValueToMetadata()
    {
        var error = Error.Technical("SVC.ERROR", "Something failed.").WithMetadata("traceId", "abc-123");

        error.Metadata.Should().ContainKey("traceId").WhoseValue.Should().Be("abc-123");
    }

    [Fact]
    public void WithMetadata_OverwritesExistingKey()
    {
        var error = Error.Technical("SVC.ERROR", "msg").WithMetadata("key", "original").WithMetadata("key", "updated");

        error.Metadata["key"]!.Should().Be("updated");
    }

    [Fact]
    public void WithMetadata_DoesNotMutateOriginal()
    {
        var original = Error.Technical("SVC.ERROR", "msg");
        var enriched = original.WithMetadata("key", "value");

        original.Metadata.Should().BeEmpty();
        enriched.Metadata.Should().HaveCount(1);
    }

    // ── WithSeverity ──────────────────────────────────────────────────────────

    [Fact]
    public void WithSeverity_ReturnsCopyWithNewSeverity()
    {
        var error = Error.Technical("SVC.ERROR", "msg").WithSeverity(ErrorSeverity.Critical);

        error.Severity.Should().Be(ErrorSeverity.Critical);
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void Equality_SameCodeAndCategory_AreEqual()
    {
        var a = Error.Validation("USER.INVALID", "msg1");
        var b = Error.Validation("USER.INVALID", "msg2"); // same code + category

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentCode_AreNotEqual()
    {
        var a = Error.Validation("USER.A", "msg");
        var b = Error.Validation("USER.B", "msg");

        a.Should().NotBe(b);
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ContainsCodeAndMessage()
    {
        var error = Error.NotFound("ORDER.NOT_FOUND", "Order not found.");

        error.ToString().Should().Contain("ORDER.NOT_FOUND").And.Contain("Order not found.");
    }

    [Fact]
    public void ToString_WithMetadata_IncludesMetadata()
    {
        var error = Error.Technical("SVC.ERROR", "msg").WithMetadata("traceId", "xyz");

        error.ToString().Should().Contain("traceId").And.Contain("xyz");
    }
}
