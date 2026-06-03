namespace SurveyCalcKit.Core.Models;

public sealed record LevelingRouteResult(
    string StartBenchmarkName,
    string EndBenchmarkName,
    double StartElevation,
    double EndElevation,
    double SumBacksight,
    double SumForesight,
    double ObservedHeightDifference,
    double KnownHeightDifference,
    double ClosureError,
    int StationCount,
    double CorrectionPerStation,
    List<LevelingPointResult> Points,
    List<string> Warnings);
