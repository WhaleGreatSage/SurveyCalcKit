namespace SurveyCalcKit.Core.Models;

public sealed record ClothoidResult(
    string CurveName,
    double StartX,
    double StartY,
    double StartAzimuthDegrees,
    double Radius,
    double SpiralLength,
    double SpiralParameterA,
    double SpiralAngleDegrees,
    double Shift,
    string TurnDirection,
    List<ClothoidPointResult> Points,
    List<string> Warnings);
