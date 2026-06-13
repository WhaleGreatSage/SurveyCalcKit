namespace SurveyCalcKit.Core.Models;

public sealed record CircularCurveResult(
    string CurveName,
    double PiChainage,
    double Radius,
    double DeflectionAngleDegrees,
    string TurnDirection,
    double TangentLength,
    double CurveLength,
    double ExternalDistance,
    double MiddleOrdinate,
    double PcChainage,
    double PtChainage,
    List<string> Warnings);
