using SurveyCalcKit.Core.Models;
using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class DxfExporterTests
{
    [Fact]
    public void Export_CreatesDxfFile()
    {
        var outputPath = CreateTempDxfPath();
        try
        {
            var result = new DxfExporter().Export(CreatePoints(), outputPath, CreateOptions());

            Assert.True(File.Exists(outputPath));
            Assert.Equal(outputPath, result.OutputPath);
            Assert.Equal(4, result.PointCount);
        }
        finally
        {
            DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public void Export_WritesSectionAndEntities()
    {
        var text = ExportAndRead(CreatePoints());

        Assert.Contains("SECTION", text);
        Assert.Contains("ENTITIES", text);
        Assert.Contains("ENDSEC", text);
        Assert.Contains("EOF", text);
    }

    [Fact]
    public void Export_WritesPointEntities()
    {
        var text = ExportAndRead(CreatePoints());

        Assert.Contains("POINT", text);
    }

    [Fact]
    public void Export_WritesTextLabels()
    {
        var text = ExportAndRead(CreatePoints());

        Assert.Contains("TEXT", text);
        Assert.Contains("P1", text);
    }

    [Fact]
    public void Export_WritesPolylineEntity()
    {
        var text = ExportAndRead(CreatePoints());

        Assert.Contains("LWPOLYLINE", text);
    }

    [Fact]
    public void Export_AddsWarningWhenTooFewPointsForPolyline()
    {
        var outputPath = CreateTempDxfPath();
        try
        {
            var result = new DxfExporter().Export(
                new[] { new PointRecord("P1", 1000, 1000) },
                outputPath,
                CreateOptions(exportPolyline: true));

            Assert.Contains(result.Warnings, warning => warning.Contains("at least two points", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            DeleteIfExists(outputPath);
        }
    }

    [Fact]
    public void Export_PreservesUtf8ChineseLabels()
    {
        var text = ExportAndRead(new[] { new PointRecord("点1", 1000, 1000), new PointRecord("P2", 1010, 1000) });

        Assert.Contains("点1", text);
    }

    private static string ExportAndRead(IReadOnlyList<PointRecord> points)
    {
        var outputPath = CreateTempDxfPath();
        try
        {
            new DxfExporter().Export(points, outputPath, CreateOptions());
            return File.ReadAllText(outputPath);
        }
        finally
        {
            DeleteIfExists(outputPath);
        }
    }

    private static IReadOnlyList<PointRecord> CreatePoints()
    {
        return new[]
        {
            new PointRecord("P1", 1000, 1000),
            new PointRecord("P2", 1050, 1000),
            new PointRecord("P3", 1050, 1040),
            new PointRecord("P4", 1000, 1040)
        };
    }

    private static DxfExportOptions CreateOptions(bool exportPolyline = true)
    {
        return new DxfExportOptions("SurveyCalcKit", true, true, exportPolyline, false, 2.5);
    }

    private static string CreateTempDxfPath()
    {
        return Path.Combine(Path.GetTempPath(), $"surveycalckit-{Guid.NewGuid():N}.dxf");
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
