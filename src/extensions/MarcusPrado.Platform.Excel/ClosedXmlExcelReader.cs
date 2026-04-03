using ClosedXML.Excel;
using MarcusPrado.Platform.Abstractions.Documents;

namespace MarcusPrado.Platform.Excel;

/// <summary>Reads Excel workbooks using ClosedXML.</summary>
public sealed class ClosedXmlExcelReader : IExcelReader
{
    /// <inheritdoc />
    public Task<IReadOnlyList<IReadOnlyList<string?>>> ReadAsync(
        byte[] excelBytes,
        int sheetIndex = 1,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(excelBytes);

        if (sheetIndex < 1)
            throw new ArgumentOutOfRangeException(nameof(sheetIndex), "Sheet index must be ≥ 1.");

        using var stream    = new MemoryStream(excelBytes);
        using var workbook  = new XLWorkbook(stream);

        if (sheetIndex > workbook.Worksheets.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sheetIndex),
                $"Workbook has {workbook.Worksheets.Count} sheet(s); requested index {sheetIndex}.");
        }

        var worksheet = workbook.Worksheet(sheetIndex);
        var usedRange = worksheet.RangeUsed();

        if (usedRange is null)
            return Task.FromResult<IReadOnlyList<IReadOnlyList<string?>>>([]);

        var result = new List<IReadOnlyList<string?>>();
        foreach (var row in usedRange.Rows())
        {
            var cells = new List<string?>(row.CellCount());
            foreach (var cell in row.Cells())
            {
                cells.Add(cell.IsEmpty() ? null : cell.GetString());
            }
            result.Add(cells);
        }

        return Task.FromResult<IReadOnlyList<IReadOnlyList<string?>>>(result);
    }
}
