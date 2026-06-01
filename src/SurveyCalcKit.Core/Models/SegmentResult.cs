namespace SurveyCalcKit.Core.Models;

public sealed record SegmentResult(
    string From,
    string To,
    double Dx,
    double Dy,
    double Distance2D,
    double? Distance3D,
    double AzimuthDegrees,
    double? DeltaH,
    double? SlopePercent);
