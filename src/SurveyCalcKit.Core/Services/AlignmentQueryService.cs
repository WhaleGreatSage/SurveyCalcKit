using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class AlignmentQueryService
{
    private const double ChainageTolerance = 1e-8;

    public AlignmentQueryResult Query(AlignmentQueryInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        var points = new List<AlignmentQueryPointResult>();
        var alignment = input.Alignment;
        if (alignment.Elements.Count == 0)
        {
            warnings.Add("Alignment has no elements to query.");
            return new AlignmentQueryResult(alignment.AlignmentName, points, warnings);
        }

        foreach (var duplicate in input.Chainages.GroupBy(value => value).Where(group => group.Count() > 1))
        {
            warnings.Add($"Duplicate query chainage: {duplicate.Key:0.###}.");
        }

        var start = alignment.Elements[0].StartChainage;
        var end = alignment.Elements[^1].EndState.Chainage;
        foreach (var chainage in input.Chainages)
        {
            var element = alignment.Elements.FirstOrDefault(candidate =>
                chainage >= candidate.StartChainage - ChainageTolerance &&
                chainage <= candidate.EndState.Chainage + ChainageTolerance);
            if (element is null)
            {
                warnings.Add($"Query chainage {chainage:0.###} is outside alignment range {start:0.###} to {end:0.###}.");
                points.Add(new AlignmentQueryPointResult(chainage, 0, 0, 0, 0, double.PositiveInfinity, string.Empty, string.Empty, false));
                continue;
            }

            var localDistance = Math.Clamp(chainage - element.StartChainage, 0, element.Length);
            var state = element.GetStateAt(localDistance);
            var radius = Math.Abs(state.Curvature) <= AlignmentMath.CurvatureTolerance
                ? double.PositiveInfinity
                : 1.0 / Math.Abs(state.Curvature);
            points.Add(new AlignmentQueryPointResult(
                chainage,
                state.X,
                state.Y,
                state.AzimuthDegrees,
                state.Curvature,
                radius,
                state.ElementType,
                state.ElementName,
                true));
        }

        return new AlignmentQueryResult(alignment.AlignmentName, points, warnings);
    }
}
