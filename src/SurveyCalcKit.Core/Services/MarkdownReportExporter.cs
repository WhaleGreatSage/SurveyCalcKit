using System.Text;
using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class MarkdownReportExporter
{
    public MarkdownExportResult Export(string title, string reportContent, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(reportContent);
        ArgumentNullException.ThrowIfNull(outputPath);

        var warnings = new List<string>();
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(reportContent))
        {
            warnings.Add("Report content is empty; Markdown file was not generated.");
            errors.Add("Report content is empty.");
            return new MarkdownExportResult(outputPath, warnings, errors);
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            errors.Add("Output path is required.");
            return new MarkdownExportResult(outputPath, warnings, errors);
        }

        try
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var builder = new StringBuilder();
            builder.AppendLine($"# {EscapeTitle(title)}");
            builder.AppendLine();
            builder.AppendLine($"Generated: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            builder.AppendLine();
            builder.AppendLine("```text");
            builder.AppendLine(reportContent.TrimEnd());
            builder.AppendLine("```");

            File.WriteAllText(outputPath, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            errors.Add($"Could not export Markdown report: {ex.Message}");
        }

        return new MarkdownExportResult(outputPath, warnings, errors);
    }

    private static string EscapeTitle(string title)
    {
        return string.IsNullOrWhiteSpace(title)
            ? "SurveyCalcKit Report"
            : title.Replace("#", "\\#", StringComparison.Ordinal).Trim();
    }
}
