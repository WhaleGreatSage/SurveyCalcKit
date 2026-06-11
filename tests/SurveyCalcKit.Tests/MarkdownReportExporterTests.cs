using SurveyCalcKit.Core.Services;

namespace SurveyCalcKit.Tests;

public class MarkdownReportExporterTests
{
    [Fact]
    public void Export_GeneratesMarkdownFileWithTitleAndTimestamp()
    {
        using var temp = new TemporaryDirectory();
        var path = Path.Combine(temp.Path, "report.md");
        var exporter = new MarkdownReportExporter();

        var result = exporter.Export("Survey Report", "Point count: 4", path);

        Assert.True(result.IsSuccess);
        var markdown = File.ReadAllText(path);
        Assert.Contains("# Survey Report", markdown);
        Assert.Contains("Generated:", markdown);
        Assert.Contains("Point count: 4", markdown);
    }

    [Fact]
    public void Export_PreservesUtf8ChineseContent()
    {
        using var temp = new TemporaryDirectory();
        var path = Path.Combine(temp.Path, "report.md");
        var exporter = new MarkdownReportExporter();

        var result = exporter.Export("中文报告", "总长度: 135.320", path);

        Assert.True(result.IsSuccess);
        var markdown = File.ReadAllText(path);
        Assert.Contains("中文报告", markdown);
        Assert.Contains("总长度", markdown);
    }

    [Fact]
    public void Export_ReturnsWarningForEmptyReport()
    {
        using var temp = new TemporaryDirectory();
        var path = Path.Combine(temp.Path, "report.md");
        var exporter = new MarkdownReportExporter();

        var result = exporter.Export("Empty Report", "", path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Warnings, warning => warning.Contains("empty", StringComparison.OrdinalIgnoreCase));
        Assert.False(File.Exists(path));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"SurveyCalcKit-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
