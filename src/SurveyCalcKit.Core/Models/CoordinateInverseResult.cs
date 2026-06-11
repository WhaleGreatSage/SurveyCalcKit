namespace SurveyCalcKit.Core.Models;

public sealed record CoordinateInverseResult(
    string FromPointName,
    string ToPointName,
    double DeltaX,
    double DeltaY,
    double Distance2D,
    double AzimuthDegrees,
    double? DeltaH,
    double? Distance3D,
    List<string> Warnings);
