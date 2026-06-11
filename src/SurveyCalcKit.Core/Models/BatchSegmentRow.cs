namespace SurveyCalcKit.Core.Models;

public sealed record BatchSegmentRow(
    int Index,
    string From,
    string To,
    double DeltaX,
    double DeltaY,
    double Distance2D,
    double AzimuthDegrees,
    double CumulativeDistance,
    double? DeltaH,
    double? SlopePercent);
