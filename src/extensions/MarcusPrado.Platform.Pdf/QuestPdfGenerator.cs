using System.Text.Json;
using MarcusPrado.Platform.Abstractions.Documents;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MarcusPrado.Platform.Pdf;

/// <summary>Generates PDF documents using the QuestPDF library.</summary>
public sealed class QuestPdfGenerator : IPdfGenerator
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    private readonly QuestPdfTemplateRegistry _registry;

    /// <summary>Initializes a new instance of <see cref="QuestPdfGenerator"/>.</summary>
    public QuestPdfGenerator(QuestPdfTemplateRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    /// <inheritdoc />
    public Task<byte[]> GenerateAsync(PdfTemplate template, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(template);

        var bytes = _registry.TryGetComposer(template.TemplateName, out var composer)
            ? GenerateWithComposer(template, composer!)
            : GenerateDefault(template);

        return Task.FromResult(bytes);
    }

    private static byte[] GenerateWithComposer(PdfTemplate template, Action<PageDescriptor, object> composer)
    {
        var pageSize = MapPageSize(template.PageSize, template.Landscape);

        return Document
            .Create(container =>
                container.Page(page =>
                {
                    page.Size(pageSize);
                    page.Margin(2, Unit.Centimetre);
                    composer(page, template.Data);
                })
            )
            .GeneratePdf();
    }

    private static byte[] GenerateDefault(PdfTemplate template)
    {
        var json = JsonSerializer.Serialize(template.Data, _jsonOptions);

        var pageSize = MapPageSize(template.PageSize, template.Landscape);

        return Document
            .Create(container =>
                container.Page(page =>
                {
                    page.Size(pageSize);
                    page.Margin(2, Unit.Centimetre);
                    page.Content()
                        .Column(col =>
                        {
                            col.Item().Text(template.TemplateName).Bold().FontSize(14);
                            col.Item().LineHorizontal(1);
                            col.Item().Text(json).FontSize(10);
                        });
                })
            )
            .GeneratePdf();
    }

    private static PageSize MapPageSize(PdfPageSize size, bool landscape)
    {
        var s = size switch
        {
            PdfPageSize.A4 => PageSizes.A4,
            PdfPageSize.A3 => PageSizes.A3,
            PdfPageSize.Letter => PageSizes.Letter,
            PdfPageSize.Legal => PageSizes.Legal,
            _ => PageSizes.A4,
        };

        return landscape ? new PageSize(s.Height, s.Width) : s;
    }
}
