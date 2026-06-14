namespace SurveyCalcKit.Core.Models;

public sealed record VerticalCurveResult(
    string CurveName,
    double PviChainage,
    double PviElevation,
    double GradeInPercent,
    double GradeOutPercent,
    double AlgebraicGradeDifferencePercent,
    double CurveLength,
    string CurveType,
    double PvcChainage,
    double PvtChainage,
    double PvcElevation,
    double PvtElevation,
    List<VerticalCurvePointResult> Points,
    List<string> Warnings);
