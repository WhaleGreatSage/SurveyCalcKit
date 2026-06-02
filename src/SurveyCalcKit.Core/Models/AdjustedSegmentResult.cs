namespace SurveyCalcKit.Core.Models;

public sealed record AdjustedSegmentResult(
    string From,
    string To,
    double OriginalDx,
    double OriginalDy,
    double Distance2D,
    double CorrectionDx,
    double CorrectionDy,
    double AdjustedDx,
    double AdjustedDy);
