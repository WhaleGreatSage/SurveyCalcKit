using ClosedXML.Excel;
using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class ExcelServiceTests
{
    [Fact]
    public void ImportPoints_ReadsValidExcelWorkbook()
    {
        var path = CreatePointWorkbook();
        var service = new ExcelService();

        var result = service.ImportPoints(path);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        Assert.Equal(3, result.Points.Count);
        Assert.Equal(new PointRecord("P1", 100.000, 200.000, 15.230), result.Points[0]);
        Assert.Equal(new PointRecord("P2", 130.500, 240.750, null), result.Points[1]);
    }

    [Fact]
    public void ImportPoints_ReturnsErrorForEmptyFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");
        File.WriteAllBytes(path, Array.Empty<byte>());
        var service = new ExcelService();

        var result = service.ImportPoints(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ExportTraverseResults_GeneratesExcelFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");
        var service = new ExcelService();
        var segments = CreateTraverseSegments();

        var result = service.ExportTraverseResults(path, segments);

        Assert.True(result.IsSuccess);
        Assert.True(File.Exists(path));
        Assert.True(new FileInfo(path).Length > 0);
    }

    [Fact]
    public void ExportTraverseResults_CanBeOpenedAndMatchesCalculationResults()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");
        var service = new ExcelService();
        var segments = CreateTraverseSegments();

        var result = service.ExportTraverseResults(path, segments);

        Assert.True(result.IsSuccess);
        using var workbook = new XLWorkbook(path);
        var worksheet = workbook.Worksheet("Traverse");
        Assert.Equal("From", worksheet.Cell(1, 1).GetString());
        Assert.Equal("P1", worksheet.Cell(2, 1).GetString());
        Assert.Equal("P2", worksheet.Cell(2, 2).GetString());
        Assert.Equal(3.0, worksheet.Cell(2, 3).GetDouble(), 6);
        Assert.Equal(4.0, worksheet.Cell(2, 4).GetDouble(), 6);
        Assert.Equal(5.0, worksheet.Cell(2, 5).GetDouble(), 6);
    }

    private static string CreatePointWorkbook()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Points");
        worksheet.Cell(1, 1).Value = "Name";
        worksheet.Cell(1, 2).Value = "X";
        worksheet.Cell(1, 3).Value = "Y";
        worksheet.Cell(1, 4).Value = "H";
        worksheet.Cell(2, 1).Value = "P1";
        worksheet.Cell(2, 2).Value = 100.000;
        worksheet.Cell(2, 3).Value = 200.000;
        worksheet.Cell(2, 4).Value = 15.230;
        worksheet.Cell(3, 1).Value = "P2";
        worksheet.Cell(3, 2).Value = 130.500;
        worksheet.Cell(3, 3).Value = 240.750;
        worksheet.Cell(4, 1).Value = "P3";
        worksheet.Cell(4, 2).Value = 160.250;
        worksheet.Cell(4, 3).Value = 260.125;
        worksheet.Cell(4, 4).Value = 16.010;
        workbook.SaveAs(path);
        return path;
    }

    private static IReadOnlyList<SegmentResult> CreateTraverseSegments()
    {
        var calculator = new TraverseCalculator();
        return calculator.CalculateSegments(new[]
        {
            new PointRecord("P1", 0, 0),
            new PointRecord("P2", 3, 4)
        });
    }
}
