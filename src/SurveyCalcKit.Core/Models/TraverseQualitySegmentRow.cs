namespace SurveyCalcKit.Core.Models;

public sealed record TraverseQualitySegmentRow(
    int Index,
    string From,
    string To,
    double Dx,
    double Dy,
    double Distance,
    double AzimuthDegrees,
    double CumulativeLength);
