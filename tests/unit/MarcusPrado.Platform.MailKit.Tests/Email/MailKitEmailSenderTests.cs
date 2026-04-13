using System.IO;
using MarcusPrado.Platform.MailKit.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.MailKit.Tests.Email;

public sealed class MailKitEmailSenderTests
{
    private static MailKitEmailSender BuildSender(ISmtpClient? smtp = null)
    {
        smtp ??= Substitute.For<ISmtpClient>();
        var opts = new MailKitOptions { SmtpHost = "localhost", SmtpPort = 25 };
        return new MailKitEmailSender(opts, smtp);
    }

    [Fact]
    public async Task SendAsync_WhenSmtpThrows_ReturnsFailureResult()
    {
        var smtp = Substitute.For<ISmtpClient>();
        smtp.ConnectAsync(Arg.Any<string>(), Arg.Any<int>(),
                Arg.Any<SecureSocketOptions>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new InvalidOperationException("SMTP unavailable"));

        var sender = BuildSender(smtp);
        var message = new EmailMessage("test@example.com", "Subject", "Body");

        var result = await sender.SendAsync(message);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("SMTP unavailable");
    }

    [Fact]
    public async Task SendBulkAsync_EmptyList_ReturnsSuccessImmediately()
    {
        var smtp = Substitute.For<ISmtpClient>();
        var sender = BuildSender(smtp);

        var result = await sender.SendBulkAsync(Array.Empty<EmailMessage>());

        result.Success.Should().BeTrue();
        await smtp.DidNotReceiveWithAnyArgs()
            .ConnectAsync(default!, default, default(SecureSocketOptions), default);
    }

    [Fact]
    public void MailKitOptions_DefaultValues_AreReasonable()
    {
        var opts = new MailKitOptions();

        opts.SmtpPort.Should().Be(587);
        opts.UseSsl.Should().BeFalse();
        opts.DefaultFrom.Should().NotBeNullOrWhiteSpace();
        opts.TemplateDirectory.Should().Be("Templates");
    }
}

public sealed class SimpleTemplateRendererTests
{
    [Fact]
    public async Task RenderAsync_ReplacesTokens_FromModel()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "welcome.html"), "Hello {{Name}}, your code is {{Code}}.");

        var opts = new MailKitOptions { TemplateDirectory = dir };
        var renderer = new SimpleTemplateRenderer(opts);

        var result = await renderer.RenderAsync("welcome", new { Name = "Alice", Code = "XYZ" });

        result.Should().Be("Hello Alice, your code is XYZ.");
        Directory.Delete(dir, recursive: true);
    }

    [Fact]
    public async Task RenderAsync_MissingTemplate_ThrowsFileNotFoundException()
    {
        var opts = new MailKitOptions { TemplateDirectory = Path.GetTempPath() };
        var renderer = new SimpleTemplateRenderer(opts);

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => renderer.RenderAsync("nonexistent", new { }));
    }
}

public sealed class MailKitExtensionsTests
{
    [Fact]
    public void AddPlatformMailKit_RegistersEmailSenderAndRenderer()
    {
        var services = new ServiceCollection();
        services.AddPlatformMailKit(o => o.SmtpHost = "smtp.test.com");
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<MarcusPrado.Platform.Abstractions.Email.IEmailSender>()
            .Should().BeOfType<MailKitEmailSender>();
        sp.GetRequiredService<MarcusPrado.Platform.Abstractions.Email.IEmailTemplateRenderer>()
            .Should().BeOfType<SimpleTemplateRenderer>();
    }
}

public sealed class MailKitEmailSenderConstructorTests
{
    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        Action act = () => new MailKitEmailSender(null!, Substitute.For<ISmtpClient>());
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithNullSmtp_ThrowsArgumentNullException()
    {
        var opts = new MailKitOptions { SmtpHost = "localhost" };
        Action act = () => new MailKitEmailSender(opts, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("smtp");
    }
}
