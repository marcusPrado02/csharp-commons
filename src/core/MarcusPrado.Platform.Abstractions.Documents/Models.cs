namespace MarcusPrado.Platform.Abstractions.Documents;

/// <summary>Template descriptor for PDF generation.</summary>
public sealed record PdfTemplate(
    string TemplateName,
    object Data,
    PdfPageSize PageSize = PdfPageSize.A4,
    bool Landscape = false
);

/// <summary>Data for an Excel document export.</summary>
public sealed record ExcelDocument(
    string SheetName,
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string?>> Rows
);

/// <summary>Standard paper sizes for PDF generation.</summary>
public enum PdfPageSize
{
    /// <summary>ISO A4 (210 × 297 mm).</summary>
    A4,

    /// <summary>ISO A3 (297 × 420 mm).</summary>
    A3,

    /// <summary>US Letter (8.5 × 11 in).</summary>
    Letter,

    /// <summary>US Legal (8.5 × 14 in).</summary>
    Legal,
}
