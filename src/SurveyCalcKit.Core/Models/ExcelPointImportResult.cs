namespace SurveyCalcKit.Core.Models;

public sealed class ExcelPointImportResult
{
    public ExcelPointImportResult(
        IReadOnlyList<PointRecord> points,
        IReadOnlyList<string> errors,
        IReadOnlyList<string> warnings)
    {
        Points = points;
        Errors = errors;
        Warnings = warnings;
    }

    public IReadOnlyList<PointRecord> Points { get; }

    public IReadOnlyList<string> Errors { get; }

    public IReadOnlyList<string> Warnings { get; }

    public bool IsSuccess => Errors.Count == 0;
}
