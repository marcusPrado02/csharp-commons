using MarcusPrado.Platform.AspNetCore.Sanitization;
using FluentAssertions;
using Xunit;

namespace MarcusPrado.Platform.AspNetCore.Tests;

public sealed class InputSanitizationTests
{
    private readonly HtmlSanitizerAdapter _sanitizer = new();

    // ── HtmlSanitizerAdapter.SanitizeHtml ────────────────────────────────────

    [Fact]
    public void SanitizeHtml_RemovesScriptTags()
    {
        var result = _sanitizer.SanitizeHtml("<script>alert('xss')</script>Hello");
        result.Should().NotContain("<script>");
        result.Should().Contain("Hello");
    }

    [Fact]
    public void SanitizeHtml_PreservesSafeBoldAndItalicTags()
    {
        var result = _sanitizer.SanitizeHtml("<b>Bold</b> and <i>Italic</i>");
        result.Should().Contain("<b>Bold</b>");
        result.Should().Contain("<i>Italic</i>");
    }

    [Fact]
    public void SanitizeHtml_RemovesOnClickAttribute()
    {
        var result = _sanitizer.SanitizeHtml("<div onclick=\"alert('xss')\">Click me</div>");
        result.Should().NotContain("onclick");
        result.Should().Contain("Click me");
    }

    [Fact]
    public void SanitizeHtml_HandlesEmptyString_NoException()
    {
        var act = () => _sanitizer.SanitizeHtml(string.Empty);
        act.Should().NotThrow();
        var result = _sanitizer.SanitizeHtml(string.Empty);
        result.Should().BeEmpty();
    }

    // ── HtmlSanitizerAdapter.StripHtml ───────────────────────────────────────

    [Fact]
    public void StripHtml_RemovesAllTagsReturningPlainText()
    {
        var result = _sanitizer.StripHtml("<b>Hello</b> <i>World</i> <script>alert(1)</script>");
        result.Should().NotContain("<");
        result.Should().NotContain(">");
        result.Should().Contain("Hello");
        result.Should().Contain("World");
    }

    // ── SqlInjectionDetector ─────────────────────────────────────────────────

    [Fact]
    public void SqlInjectionDetector_DetectsDropTablePattern()
    {
        SqlInjectionDetector.ContainsSqlInjection("'; DROP TABLE users; --")
            .Should().BeTrue();
    }

    [Fact]
    public void SqlInjectionDetector_DetectsSelectPattern()
    {
        SqlInjectionDetector.ContainsSqlInjection("SELECT * FROM users")
            .Should().BeTrue();
    }

    [Fact]
    public void SqlInjectionDetector_ReturnsFalseForSafeInput()
    {
        SqlInjectionDetector.ContainsSqlInjection("hello world")
            .Should().BeFalse();
    }

    [Fact]
    public void SqlInjectionDetector_ReturnsFalseForNull()
    {
        SqlInjectionDetector.ContainsSqlInjection(null)
            .Should().BeFalse();
    }
}
