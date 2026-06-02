namespace SurveyCalcKit.Core.Models;

public sealed record AdjustedPointRecord(
    string Name,
    double OriginalX,
    double OriginalY,
    double AdjustedX,
    double AdjustedY,
    double CorrectionX,
    double CorrectionY,
    double? H = null);
