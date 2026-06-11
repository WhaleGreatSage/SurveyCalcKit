namespace SurveyCalcKit.Core.Models;

public sealed record BatchSegmentTableResult(
    int PointCount,
    int SegmentCount,
    double TotalLength,
    List<BatchSegmentRow> Rows,
    List<string> Warnings);
