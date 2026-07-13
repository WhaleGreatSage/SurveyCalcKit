namespace SurveyCalcKit.Core.Models;

public sealed record ClothoidPointResult(
    double DistanceFromStart,
    double X,
    double Y,
    double AzimuthDegrees,
    double Curvature,
    double RadiusAtPoint,
    bool IsInsideSpiral);
