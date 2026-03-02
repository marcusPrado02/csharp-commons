namespace MarcusPrado.Platform.Serilog.Tests.Sanitizer;

public sealed class LogSanitizerTests
{
    [Fact]
    public void Sanitize_EmptyString_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, LogSanitizer.Sanitize(string.Empty));
    }

    [Fact]
    public void Sanitize_NullString_ReturnsNull()
    {
        Assert.Null(LogSanitizer.Sanitize(null!));
    }

    [Fact]
    public void Sanitize_WithEmailAddress_RedactsEmail()
    {
        var input = "User login: john.doe@example.com attempted";
        var result = LogSanitizer.Sanitize(input);
        Assert.DoesNotContain("john.doe@example.com", result);
        Assert.Contains("***@***.***", result);
    }

    [Fact]
    public void Sanitize_WithEmbeddedEmail_KeepsSurroundingText()
    {
        var input = "Error for user@domain.org: bad password";
        var result = LogSanitizer.Sanitize(input);
        Assert.StartsWith("Error for ", result);
        Assert.Contains("bad password", result);
        Assert.DoesNotContain("user@domain.org", result);
    }

    [Fact]
    public void Sanitize_PlainText_Unchanged()
    {
        const string input = "User logged in successfully";
        Assert.Equal(input, LogSanitizer.Sanitize(input));
    }

    [Fact]
    public void Sanitize_MultipleEmails_RedactsAll()
    {
        var input = "From: a@a.com to b@b.com";
        var result = LogSanitizer.Sanitize(input);
        Assert.Equal(2, result.Split("***@***.***").Length - 1);
    }
}
