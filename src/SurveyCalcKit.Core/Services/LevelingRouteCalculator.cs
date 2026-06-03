using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class LevelingRouteCalculator
{
    private const double SuspiciousClosureErrorThreshold = 0.05;

    public LevelingRouteResult Calculate(LevelingRouteInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        var stationCount = input.Observations.Count;
        var sumBacksight = input.Observations.Sum(observation => observation.Backsight);
        var sumForesight = input.Observations.Sum(observation => observation.Foresight);
        var observedHeightDifference = sumBacksight - sumForesight;
        var knownHeightDifference = input.EndElevation - input.StartElevation;
        var closureError = observedHeightDifference - knownHeightDifference;
        var correctionPerStation = stationCount == 0 ? 0 : -closureError / stationCount;

        if (stationCount == 0)
        {
            warnings.Add("Leveling route requires at least one observation; station count is zero.");
        }

        if (input.Observations.Any(observation => observation.Backsight < 0 || observation.Foresight < 0))
        {
            warnings.Add("Leveling route contains negative sight values.");
        }

        if (Math.Abs(closureError) > SuspiciousClosureErrorThreshold)
        {
            warnings.Add($"Closure error {closureError:0.###} is suspiciously large for a beginner leveling route.");
        }

        var points = new List<LevelingPointResult>(stationCount);
        var rawElevation = input.StartElevation;
        for (var i = 0; i < input.Observations.Count; i++)
        {
            var observation = input.Observations[i];
            rawElevation += observation.Backsight - observation.Foresight;
            var correction = correctionPerStation * (i + 1);
            points.Add(new LevelingPointResult(
                observation.PointName,
                rawElevation,
                correction,
                rawElevation + correction));
        }

        return new LevelingRouteResult(
            input.StartBenchmarkName,
            input.EndBenchmarkName,
            input.StartElevation,
            input.EndElevation,
            sumBacksight,
            sumForesight,
            observedHeightDifference,
            knownHeightDifference,
            closureError,
            stationCount,
            correctionPerStation,
            points,
            warnings);
    }
}
