namespace SurveyCalcKit.Core.Models;

public sealed record LevelingPointResult(
    string PointName,
    double RawElevation,
    double Correction,
    double AdjustedElevation);
