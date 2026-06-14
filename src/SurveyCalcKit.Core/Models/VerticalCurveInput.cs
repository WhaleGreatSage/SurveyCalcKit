namespace SurveyCalcKit.Core.Models;

public sealed record VerticalCurveInput(
    string CurveName,
    double PviChainage,
    double PviElevation,
    double GradeInPercent,
    double GradeOutPercent,
    double CurveLength,
    List<double> DesignChainages);
