using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Pdf.Tests;

public sealed class QuestPdfGeneratorTests
{
    static QuestPdfGeneratorTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static QuestPdfGenerator BuildGenerator(
        Action<QuestPdfTemplateRegistry>? configure = null)
    {
        var registry = new QuestPdfTemplateRegistry();
        configure?.Invoke(registry);
        return new QuestPdfGenerator(registry);
    }

    [Fact]
    public async Task GenerateAsync_DefaultTemplate_ReturnsPdfBytes()
    {
        var generator = BuildGenerator();
        var template = new PdfTemplate("invoice", new { OrderId = 1, Total = 99.99m });

        var bytes = await generator.GenerateAsync(template);

        bytes.Should().NotBeEmpty();
        Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }

    [Fact]
    public async Task GenerateAsync_LandscapeOrientation_GeneratesValidPdf()
    {
        var generator = BuildGenerator();
        var template = new PdfTemplate("report", new { }, Landscape: true);

        var bytes = await generator.GenerateAsync(template);

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateAsync_RegisteredComposer_IsInvoked()
    {
        var composerCalled = false;
        var generator = BuildGenerator(r =>
            r.Register("custom", (page, _) =>
            {
                composerCalled = true;
                page.Content().Text("Custom content");
            }));

        var template = new PdfTemplate("custom", new { });
        await generator.GenerateAsync(template);

        composerCalled.Should().BeTrue();
    }

    [Fact]
    public void TemplateRegistry_RegisterAndRetrieves_Composer()
    {
        var registry = new QuestPdfTemplateRegistry();
        registry.Register("test", (_, _) => { });

        var found = registry.TryGetComposer("TEST", out var composer);

        found.Should().BeTrue();
        composer.Should().NotBeNull();
    }
}

public sealed class QuestPdfTemplateRegistryTests
{
    [Fact]
    public void TryGetComposer_UnregisteredTemplate_ReturnsFalse()
    {
        var registry = new QuestPdfTemplateRegistry();

        var found = registry.TryGetComposer("nonexistent", out var composer);

        found.Should().BeFalse();
        composer.Should().BeNull();
    }
}

public sealed class PdfExtensionsTests
{
    [Fact]
    public void AddPlatformPdf_RegistersIPdfGenerator()
    {
        var services = new ServiceCollection();
        services.AddPlatformPdf();
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IPdfGenerator>()
            .Should().BeOfType<QuestPdfGenerator>();
    }
}
