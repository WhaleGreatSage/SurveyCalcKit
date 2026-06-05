namespace SurveyCalcKit.Core.Models;

public sealed class ExcelExportResult
{
    public ExcelExportResult(string filePath, IReadOnlyList<string> errors, IReadOnlyList<string> warnings)
    {
        FilePath = filePath;
        Errors = errors;
        Warnings = warnings;
    }

    public string FilePath { get; }

    public IReadOnlyList<string> Errors { get; }

    public IReadOnlyList<string> Warnings { get; }

    public bool IsSuccess => Errors.Count == 0;
}
