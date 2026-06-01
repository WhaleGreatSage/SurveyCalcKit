using SurveyCalcKit.Core.Models;

namespace SurveyCalcKit.Core.Services;

public sealed class TraverseCalculator
{
    private readonly ElevationCalculator elevationCalculator = new();

    public IReadOnlyList<SegmentResult> CalculateSegments(IEnumerable<PointRecord> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        var pointList = points.ToList();
        var segments = new List<SegmentResult>(Math.Max(0, pointList.Count - 1));

        for (var i = 0; i < pointList.Count - 1; i++)
        {
            var from = pointList[i];
            var to = pointList[i + 1];
            var dx = to.X - from.X;
            var dy = to.Y - from.Y;
            var distance2D = Math.Sqrt(dx * dx + dy * dy);
            var deltaH = elevationCalculator.CalculateDeltaH(from, to);
            var distance3D = deltaH.HasValue
                ? Math.Sqrt(distance2D * distance2D + deltaH.Value * deltaH.Value)
                : (double?)null;
            var slopePercent = elevationCalculator.CalculateSlopePercent(deltaH, distance2D);
            var azimuth = CalculateAzimuthDegrees(dx, dy);

            segments.Add(new SegmentResult(
                from.Name,
                to.Name,
                dx,
                dy,
                distance2D,
                distance3D,
                azimuth,
                deltaH,
                slopePercent));
        }

        return segments;
    }

    public double CalculateTotal2DLength(IEnumerable<SegmentResult> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);

        return segments.Sum(segment => segment.Distance2D);
    }

    public double CalculateAzimuthDegrees(double dx, double dy)
    {
        if (dx == 0 && dy == 0)
        {
            return 0;
        }

        var degrees = Math.Atan2(dy, dx) * 180.0 / Math.PI;
        return degrees < 0 ? degrees + 360.0 : degrees;
    }
}
