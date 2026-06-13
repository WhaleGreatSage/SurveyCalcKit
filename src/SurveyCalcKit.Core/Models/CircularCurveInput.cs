namespace SurveyCalcKit.Core.Models;

public sealed record CircularCurveInput(
    string CurveName,
    double PiChainage,
    double Radius,
    double DeflectionAngleDegrees,
    string TurnDirection);
