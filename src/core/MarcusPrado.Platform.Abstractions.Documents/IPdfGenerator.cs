namespace MarcusPrado.Platform.Abstractions.Documents;

/// <summary>Generates PDF documents from templates.</summary>
public interface IPdfGenerator
{
    /// <summary>Generates a PDF from the given template and returns raw bytes.</summary>
    Task<byte[]> GenerateAsync(PdfTemplate template, CancellationToken ct = default);
}

/// <summary>Writes data to an Excel workbook.</summary>
public interface IExcelWriter
{
    /// <summary>Writes the given document to an Excel file and returns raw bytes.</summary>
    Task<byte[]> WriteAsync(ExcelDocument document, CancellationToken ct = default);
}

/// <summary>Reads data from an Excel workbook.</summary>
public interface IExcelReader
{
    /// <summary>Reads rows from the specified sheet (1-based) of an Excel file.</summary>
    Task<IReadOnlyList<IReadOnlyList<string?>>> ReadAsync(
        byte[] excelBytes,
        int sheetIndex = 1,
        CancellationToken ct = default);
}
