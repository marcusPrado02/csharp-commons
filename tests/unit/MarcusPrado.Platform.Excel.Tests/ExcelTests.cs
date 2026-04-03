using Microsoft.Extensions.DependencyInjection;

namespace MarcusPrado.Platform.Excel.Tests;

public sealed class ClosedXmlExcelWriterTests
{
    [Fact]
    public async Task WriteAsync_ReturnsNonEmptyBytes()
    {
        var writer = new ClosedXmlExcelWriter();
        var doc    = new ExcelDocument(
            "Sheet1",
            ["Name", "Age"],
            [["Alice", "30"], ["Bob", "25"]]);

        var bytes = await writer.WriteAsync(doc);

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task WriteAsync_EmptyRows_StillWritesHeaders()
    {
        var writer = new ClosedXmlExcelWriter();
        var doc = new ExcelDocument("Data", ["Col1", "Col2"], []);

        var bytes = await writer.WriteAsync(doc);

        bytes.Should().NotBeEmpty();
    }
}

public sealed class ClosedXmlExcelReaderTests
{
    [Fact]
    public async Task RoundTrip_WriteAndRead_DataMatches()
    {
        var writer = new ClosedXmlExcelWriter();
        var reader = new ClosedXmlExcelReader();

        var doc = new ExcelDocument(
            "TestSheet",
            ["Name", "Score"],
            [["Alice", "95"], ["Bob", "87"]]);

        var bytes = await writer.WriteAsync(doc);
        var rows  = await reader.ReadAsync(bytes);

        // Row 1 = headers, Row 2 = Alice, Row 3 = Bob
        rows.Should().HaveCount(3);
        rows[1][0].Should().Be("Alice");
        rows[1][1].Should().Be("95");
        rows[2][0].Should().Be("Bob");
    }

    [Fact]
    public async Task ReadAsync_InvalidSheetIndex_ThrowsArgumentOutOfRangeException()
    {
        var writer = new ClosedXmlExcelWriter();
        var reader = new ClosedXmlExcelReader();
        var bytes  = await writer.WriteAsync(new ExcelDocument("S", ["H1"], []));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => reader.ReadAsync(bytes, sheetIndex: 99));
    }

    [Fact]
    public async Task ReadAsync_SheetIndexZero_ThrowsArgumentOutOfRangeException()
    {
        var reader = new ClosedXmlExcelReader();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => reader.ReadAsync([], sheetIndex: 0));
    }
}

public sealed class ExcelExtensionsTests
{
    [Fact]
    public void AddPlatformExcel_RegistersWriterAndReader()
    {
        var services = new ServiceCollection();
        services.AddPlatformExcel();
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IExcelWriter>().Should().BeOfType<ClosedXmlExcelWriter>();
        sp.GetRequiredService<IExcelReader>().Should().BeOfType<ClosedXmlExcelReader>();
    }
}
