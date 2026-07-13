namespace SurveyCalcKit.Core.Models;

public sealed record CenterlineOffsetResult(
    int CenterlinePointCount,
    int TargetPointCount,
    List<CenterlineOffsetPointResult> Results,
    List<string> Warnings);
