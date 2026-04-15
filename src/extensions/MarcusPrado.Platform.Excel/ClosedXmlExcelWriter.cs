using ClosedXML.Excel;
using MarcusPrado.Platform.Abstractions.Documents;

namespace MarcusPrado.Platform.Excel;

/// <summary>Writes Excel workbooks using ClosedXML.</summary>
public sealed class ClosedXmlExcelWriter : IExcelWriter
{
    /// <inheritdoc />
    public Task<byte[]> WriteAsync(ExcelDocument document, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(document.SheetName);

        // Write headers (row 1)
        for (var col = 0; col < document.Headers.Count; col++)
            worksheet.Cell(1, col + 1).SetValue(document.Headers[col]);

        // Style header row
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;

        // Write data rows
        for (var row = 0; row < document.Rows.Count; row++)
        {
            var rowData = document.Rows[row];
            for (var col = 0; col < rowData.Count; col++)
                worksheet.Cell(row + 2, col + 1).SetValue(rowData[col] ?? string.Empty);
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
