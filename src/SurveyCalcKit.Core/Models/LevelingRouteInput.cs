namespace SurveyCalcKit.Core.Models;

public sealed record LevelingRouteInput(
    string StartBenchmarkName,
    double StartElevation,
    string EndBenchmarkName,
    double EndElevation,
    List<LevelingObservation> Observations);
