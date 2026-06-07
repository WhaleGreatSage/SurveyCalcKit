namespace SurveyCalcKit.Core.Models;

public sealed record CoordinateForwardInput(
    string StartPointName,
    double StartX,
    double StartY,
    double AzimuthDegrees,
    double Distance,
    string EndPointName = "END");
