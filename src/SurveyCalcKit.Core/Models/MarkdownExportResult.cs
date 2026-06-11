namespace SurveyCalcKit.Core.Models;

public sealed record MarkdownExportResult(
    string FilePath,
    List<string> Warnings,
    List<string> Errors)
{
    public bool IsSuccess => Errors.Count == 0;
}
