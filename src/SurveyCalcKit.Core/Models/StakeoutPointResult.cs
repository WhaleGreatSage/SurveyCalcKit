namespace SurveyCalcKit.Core.Models;

public sealed record StakeoutPointResult(
    string PointName,
    double Chainage,
    double Offset,
    double X,
    double Y,
    string Side);
