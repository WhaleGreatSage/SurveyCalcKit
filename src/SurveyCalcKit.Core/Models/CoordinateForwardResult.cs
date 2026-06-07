namespace SurveyCalcKit.Core.Models;

public sealed record CoordinateForwardResult(
    string StartPointName,
    double StartX,
    double StartY,
    double AzimuthDegrees,
    double Distance,
    double DeltaX,
    double DeltaY,
    string EndPointName,
    double EndX,
    double EndY,
    List<string> Warnings);
