namespace SurveyCalcKit.Core.Models;

public sealed record ClothoidInput(
    string CurveName,
    double StartX,
    double StartY,
    double StartAzimuthDegrees,
    double Radius,
    double SpiralLength,
    string TurnDirection,
    List<double> DistancesFromStart);
