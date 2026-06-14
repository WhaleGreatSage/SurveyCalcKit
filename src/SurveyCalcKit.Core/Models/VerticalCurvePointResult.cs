namespace SurveyCalcKit.Core.Models;

public sealed record VerticalCurvePointResult(
    double Chainage,
    double TangentElevation,
    double CurveElevation,
    double VerticalOffset,
    bool IsInsideCurve);
