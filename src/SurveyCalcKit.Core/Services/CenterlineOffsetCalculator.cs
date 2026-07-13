using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class CenterlineOffsetCalculator
{
    private const double Tolerance = 1e-9;

    public CenterlineOffsetResult Calculate(CenterlineOffsetInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var warnings = new List<string>();
        var results = new List<CenterlineOffsetPointResult>();
        if (input.CenterlinePoints.Count < 2)
        {
            warnings.Add("Centerline requires at least two points.");
            return new CenterlineOffsetResult(input.CenterlinePoints.Count, input.TargetPoints.Count, results, warnings);
        }

        ValidateCenterline(input.CenterlinePoints, warnings);
        foreach (var target in input.TargetPoints)
        {
            var result = FindNearestProjection(input.CenterlinePoints, target, warnings);
            if (result is not null)
            {
                results.Add(result);
            }
        }

        return new CenterlineOffsetResult(input.CenterlinePoints.Count, input.TargetPoints.Count, results, warnings);
    }

    private static void ValidateCenterline(IReadOnlyList<CenterlinePoint> points, List<string> warnings)
    {
        for (var index = 1; index < points.Count; index++)
        {
            if (points[index].Chainage <= points[index - 1].Chainage)
            {
                warnings.Add($"Centerline chainage is non-increasing at {points[index].Name}.");
            }

            var dx = points[index].X - points[index - 1].X;
            var dy = points[index].Y - points[index - 1].Y;
            if (dx * dx + dy * dy <= Tolerance)
            {
                warnings.Add($"Centerline segment {points[index - 1].Name}->{points[index].Name} has zero length.");
            }
        }

        foreach (var duplicate in points.GroupBy(point => point.Chainage).Where(group => group.Count() > 1))
        {
            warnings.Add($"Duplicate centerline chainage: {duplicate.Key:0.###}.");
        }
    }

    private static CenterlineOffsetPointResult? FindNearestProjection(
        IReadOnlyList<CenterlinePoint> points,
        PointRecord target,
        List<string> warnings)
    {
        ProjectionCandidate? best = null;
        var ambiguous = false;
        for (var index = 0; index < points.Count - 1; index++)
        {
            var candidate = Project(points[index], points[index + 1], target, index);
            if (candidate is null)
            {
                continue;
            }

            if (best is null || candidate.DistanceSquared < best.DistanceSquared - Tolerance)
            {
                best = candidate;
                ambiguous = false;
            }
            else if (Math.Abs(candidate.DistanceSquared - best.DistanceSquared) <= Tolerance)
            {
                ambiguous = true;
            }
        }

        if (best is null)
        {
            warnings.Add($"Target {target.Name} could not be projected because all centerline segments have zero length.");
            return null;
        }

        if (ambiguous)
        {
            warnings.Add($"Target {target.Name} has ambiguous equal-distance projections near a centerline vertex; the earliest segment was selected.");
        }

        if (best.DistanceSquared <= Tolerance)
        {
            warnings.Add($"Target {target.Name} lies on the centerline within tolerance.");
        }

        var signedOffset = best.Cross > Tolerance
            ? Math.Sqrt(best.DistanceSquared)
            : best.Cross < -Tolerance
                ? -Math.Sqrt(best.DistanceSquared)
                : 0;
        var side = signedOffset > 0 ? "Left" : signedOffset < 0 ? "Right" : "OnLine";
        return new CenterlineOffsetPointResult(
            target.Name,
            best.From.Chainage + best.ClampedRatio * (best.To.Chainage - best.From.Chainage),
            signedOffset,
            Math.Sqrt(best.DistanceSquared),
            side,
            best.ProjectionX,
            best.ProjectionY,
            best.SegmentIndex,
            best.From.Name,
            best.To.Name,
            Math.Sqrt(best.DistanceSquared),
            best.RawRatio >= 0 && best.RawRatio <= 1);
    }

    private static ProjectionCandidate? Project(CenterlinePoint from, CenterlinePoint to, PointRecord target, int segmentIndex)
    {
        var vx = to.X - from.X;
        var vy = to.Y - from.Y;
        var lengthSquared = vx * vx + vy * vy;
        if (lengthSquared <= Tolerance)
        {
            return null;
        }

        var wx = target.X - from.X;
        var wy = target.Y - from.Y;
        var rawRatio = (wx * vx + wy * vy) / lengthSquared;
        var clampedRatio = Math.Clamp(rawRatio, 0, 1);
        var projectionX = from.X + clampedRatio * vx;
        var projectionY = from.Y + clampedRatio * vy;
        var dx = target.X - projectionX;
        var dy = target.Y - projectionY;
        return new ProjectionCandidate(
            from,
            to,
            segmentIndex,
            rawRatio,
            clampedRatio,
            projectionX,
            projectionY,
            dx * dx + dy * dy,
            vx * wy - vy * wx);
    }

    private sealed record ProjectionCandidate(
        CenterlinePoint From,
        CenterlinePoint To,
        int SegmentIndex,
        double RawRatio,
        double ClampedRatio,
        double ProjectionX,
        double ProjectionY,
        double DistanceSquared,
        double Cross);
}
