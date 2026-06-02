namespace SurveyCalcKit.Core.Models;

public sealed record TraverseClosureResult(
    string StartPointName,
    string EndPointName,
    double StartX,
    double StartY,
    double EndX,
    double EndY,
    double Fx,
    double Fy,
    double ClosureError,
    double TotalLength,
    double RelativeClosureRatio,
    IReadOnlyList<AdjustedPointRecord> AdjustedPoints,
    IReadOnlyList<AdjustedSegmentResult> AdjustedSegments,
    IReadOnlyList<string> Warnings);
